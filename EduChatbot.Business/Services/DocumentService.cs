using System.Globalization;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using EduChatbot.Data.Repositories;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Pgvector;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using OpenXmlParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using OpenXmlText = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace EduChatbot.Business.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IDocumentUploadRules _documentUploadRules;
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<DocumentService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRealtimeService _realtimeService;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly IQuizRepository _quizRepository;
    private readonly EduChatbot.Data.ApplicationDbContext _context;

    private static readonly System.Text.RegularExpressions.Regex CjkRegex = new(
        @"[\u3040-\u30FF\u3400-\u4DBF\u4E00-\u9FFF\uAC00-\uD7AF]",
        System.Text.RegularExpressions.RegexOptions.Compiled);

    public DocumentService(
        IDocumentRepository documentRepository,
        ICourseRepository courseRepository,
        IDocumentUploadRules documentUploadRules,
        IEmbeddingService embeddingService,
        ILogger<DocumentService> logger,
        UserManager<ApplicationUser> userManager,
        IRealtimeService realtimeService,
        ICloudStorageService cloudStorageService,
        IQuizRepository quizRepository,
        EduChatbot.Data.ApplicationDbContext context)
    {
        _documentRepository = documentRepository;
        _courseRepository = courseRepository;
        _documentUploadRules = documentUploadRules;
        _embeddingService = embeddingService;
        _logger = logger;
        _userManager = userManager;
        _realtimeService = realtimeService;
        _cloudStorageService = cloudStorageService;
        _quizRepository = quizRepository;
        _context = context;
    }

    public async Task<DocumentListResult> GetDocumentsAsync(string? searchTerm = null, string? currentUserId = null, bool isAdmin = false, int? courseId = null)
    {
        var ownerFilter = isAdmin ? null : currentUserId;
        var documents = await _documentRepository.GetAllAsync(searchTerm, ownerFilter, courseId);

        return new DocumentListResult
        {
            Documents = documents,
            SearchTerm = searchTerm?.Trim() ?? string.Empty,
            TotalCount = documents.Count
        };
    }

    public async Task<DocumentDashboardSummary> GetDashboardSummaryAsync()
    {
        return await _documentRepository.GetDashboardSummaryAsync();
    }

    public async Task<Document?> GetDocumentDetailsAsync(int id, string? currentUserId = null, bool isAdmin = false)
    {
        var ownerFilter = isAdmin ? null : currentUserId;
        return await _documentRepository.GetByIdAsync(id, ownerFilter);
    }

    public async Task<DocumentUploadResult> UpdateDocumentAsync(
        int id,
        string fileName,
        string? currentUserId = null,
        bool isAdmin = false)
    {
        var validationMessage = ValidateDocumentMetadata(fileName);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return new DocumentUploadResult
            {
                IsSuccess = false,
                Message = validationMessage
            };
        }

        var ownerFilter = isAdmin ? null : currentUserId;
        var document = await _documentRepository.GetByIdAsync(id, ownerFilter);
        if (document == null)
        {
            return new DocumentUploadResult
            {
                IsSuccess = false,
                Message = "Document not found.",
                Status = DocumentStatuses.Failed
            };
        }

        document.FileName = fileName.Trim();

        await _documentRepository.UpdateAsync(document);

        if (!string.IsNullOrWhiteSpace(document.UploadedById))
        {
            await _realtimeService.NotifyMaterialChangeAsync("Update", document.UploadedById, document.UploadedBy, document.FileName);
        }

        return new DocumentUploadResult
        {
            IsSuccess = true,
            Message = "Document updated successfully.",
            DocumentId = document.Id,
            ChunkCount = document.ChunkCount,
            Status = document.Status
        };
    }

    public async Task<DocumentUploadResult> UploadDocumentAsync(
        Stream fileStream,
        string originalFileName,
        string contentType,
        long fileSize,
        string uploadedBy,
        string? uploadedById,
        string webRootPath,
        int courseId)
    {
        var safeOriginalFileName = (Path.GetFileName(originalFileName)?.Trim() ?? string.Empty)
            .Replace("\0", string.Empty)
            .Replace("\u0000", string.Empty);
        var validationMessage = ValidateFile(safeOriginalFileName, fileSize);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return new DocumentUploadResult
            {
                IsSuccess = false,
                Message = validationMessage
            };
        }

        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
        {
            return new DocumentUploadResult
            {
                IsSuccess = false,
                Message = "Course not found."
            };
        }

        if (string.IsNullOrWhiteSpace(uploadedById))
        {
            return new DocumentUploadResult
            {
                IsSuccess = false,
                Message = "Unable to determine the logged-in lecturer."
            };
        }

        if (!string.IsNullOrWhiteSpace(uploadedById))
        {
            var user = await _userManager.FindByIdAsync(uploadedById);
            if (user == null)
            {
                return new DocumentUploadResult
                {
                    IsSuccess = false,
                    Message = "Logged-in lecturer account not found."
                };
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin);
            if (!isAdmin)
            {
                // Business rule: lecturers can only upload documents for courses assigned by Admin.
                var isAssigned = await _courseRepository.IsLecturerAssignedToCourseAsync(uploadedById, courseId);
                if (!isAssigned)
                {
                    return new DocumentUploadResult
                    {
                        IsSuccess = false,
                        Message = "You are not assigned to teach this course."
                    };
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(uploadedById) &&
            await _documentRepository.ExistsByUploadedByAndFileNameAsync(uploadedById, safeOriginalFileName))
        {
            return new DocumentUploadResult
            {
                IsSuccess = false,
                Message = "You have already uploaded a document with the same file name. Please rename the file or delete the old document before uploading again.",
                Status = DocumentStatuses.Failed
            };
        }

        string? tempFilePath = null;
        string? uploadedPublicId = null;
        try
        {
            var extension = Path.GetExtension(safeOriginalFileName).ToLowerInvariant();
            tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");

            await using (var outputStream = File.Create(tempFilePath))
            {
                await fileStream.CopyToAsync(outputStream);
            }

            var extractedText = ExtractText(tempFilePath, extension);
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
                return new DocumentUploadResult
                {
                    IsSuccess = false,
                    Message = "Failed to extract text content from the file. Please check your PDF/DOCX file.",
                    Status = DocumentStatuses.Failed
                };
            }

            var documentChunks = await CreateDocumentChunksAsync(extractedText);

            string? validationResult = null;
            if (CjkRegex.IsMatch(extractedText))
            {
                validationResult = "Warning: Extracted text contains CJK (Chinese/Japanese/Korean) characters. This may cause alignment noise.";
                _logger.LogWarning("CJK characters detected in uploaded document. File: {FileName}", safeOriginalFileName);
                
                for (int i = 0; i < documentChunks.Count; i++)
                {
                    if (CjkRegex.IsMatch(documentChunks[i].Content))
                    {
                        _logger.LogWarning("  -> CJK detected in Chunk #{Index}. Snippet: {Snippet}", i, 
                            documentChunks[i].Content.Length > 200 ? documentChunks[i].Content[..200] : documentChunks[i].Content);
                    }
                }
            }

            // Upload the temp file stream to Cloudinary
            CloudUploadResultDto cloudResult;
            await using (var uploadStream = File.OpenRead(tempFilePath))
            {
                cloudResult = await _cloudStorageService.UploadFileAsync(uploadStream, safeOriginalFileName, contentType);
            }
            uploadedPublicId = cloudResult.PublicId;

            var document = new Document
            {
                FileName = safeOriginalFileName,
                StoredFileName = cloudResult.PublicId, // Cloudinary PublicId
                FilePath = cloudResult.Url,          // Cloudinary HTTPS URL
                UploadedBy = NormalizeUploadedBy(uploadedBy),
                UploadedById = uploadedById,
                ContentType = contentType,
                FileSize = fileSize,
                ExtractedText = extractedText,
                ChunkCount = documentChunks.Count,
                EmbeddingPreview = FormatEmbeddingPreview(documentChunks.FirstOrDefault()?.Embedding),
                Status = DocumentStatuses.Approved,
                UploadedAt = DateTime.UtcNow,
                CourseId = courseId,
                SubjectCode = course.Code,
                SubjectName = course.Name,
                MatchScore = null,
                ValidationResult = validationResult,
                Chunks = documentChunks
            };

            await _documentRepository.AddAsync(document);

            if (!string.IsNullOrWhiteSpace(document.UploadedById))
            {
                await _realtimeService.NotifyMaterialChangeAsync("Create", document.UploadedById, document.UploadedBy, document.FileName);
            }

            return new DocumentUploadResult
            {
                IsSuccess = true,
                Message = "Document uploaded and indexed successfully.",
                DocumentId = document.Id,
                ChunkCount = document.ChunkCount,
                Status = document.Status,
                CourseId = document.CourseId,
                CourseCode = document.SubjectCode,
                CourseName = document.SubjectName,
                FileName = document.FileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document upload/indexing failed for {FileName}", safeOriginalFileName);

            // Clean up Cloudinary file if it was uploaded but saving failed
            if (!string.IsNullOrWhiteSpace(uploadedPublicId))
            {
                try
                {
                    await _cloudStorageService.DeleteFileAsync(uploadedPublicId);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Failed to clean up orphaned Cloudinary file: {PublicId}", uploadedPublicId);
                }
            }

            var message = ex is InvalidOperationException or ArgumentException
                ? ex.Message
                : "Document processing failed. Please check the file or database.";

            return new DocumentUploadResult
            {
                IsSuccess = false,
                Message = message,
                Status = DocumentStatuses.Failed
            };
        }
        finally
        {
            // Always delete the local temp file
            if (!string.IsNullOrWhiteSpace(tempFilePath) && File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogError(deleteEx, "Failed to delete temporary file: {TempPath}", tempFilePath);
                }
            }
        }
    }

    private async Task<List<DocumentChunk>> CreateDocumentChunksAsync(string extractedText, int? documentId = null)
    {
        var chunks = ChunkText(extractedText);
        var documentChunks = new List<DocumentChunk>();

        for (var index = 0; index < chunks.Count; index++)
        {
            var chunkContent = chunks[index];
            var embedding = await _embeddingService.CreateEmbeddingAsync(chunkContent);

            documentChunks.Add(new DocumentChunk
            {
                DocumentId = documentId ?? 0,
                ChunkIndex = index,
                Content = chunkContent,
                Embedding = new Vector(embedding),
                CreatedAt = DateTime.UtcNow
            });
        }

        return documentChunks;
    }

    public async Task<bool> DeleteDocumentAsync(int id, string webRootPath, string? currentUserId = null, bool isAdmin = false)
    {
        var ownerFilter = isAdmin ? null : currentUserId;
        var document = await _documentRepository.GetByIdAsync(id, ownerFilter);
        if (document == null)
        {
            return false;
        }

        var uploadedById = document.UploadedById;

        await _documentRepository.DeleteAsync(document);

        // Delete from Cloudinary if it's a Cloudinary file with valid PublicId (Condition 2 & 8)
        if (!string.IsNullOrWhiteSpace(document.FilePath) && 
            (document.FilePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
             document.FilePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
            !string.IsNullOrWhiteSpace(document.StoredFileName))
        {
            try
            {
                await _cloudStorageService.DeleteFileAsync(document.StoredFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file from Cloudinary: {PublicId}", document.StoredFileName);
            }
        }
        else
        {
            // Fallback: Delete local file
            var physicalFilePath = Path.Combine(webRootPath, document.FilePath.TrimStart('/'));
            if (File.Exists(physicalFilePath))
            {
                try
                {
                    File.Delete(physicalFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete local file: {Path}", physicalFilePath);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(uploadedById))
        {
            await _realtimeService.NotifyMaterialChangeAsync("Delete", uploadedById, document.UploadedBy, document.FileName);
        }

        return true;
    }

    private string ValidateFile(string fileName, long fileSize)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "Please select a file to upload.";
        }

        if (fileSize <= 0)
        {
            return "Invalid uploaded file.";
        }

        if (fileSize > _documentUploadRules.MaxFileSizeBytes)
        {
            return "File size cannot exceed 10 MB.";
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_documentUploadRules.IsAllowedExtension(extension))
        {
            return "Only PDF or DOCX files are supported.";
        }

        return string.Empty;
    }

    private static string ValidateDocumentMetadata(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "File name is required.";
        }

        if (fileName.Trim().Length > 255)
        {
            return "File name cannot exceed 255 characters.";
        }

        return string.Empty;
    }

    private static string NormalizeUploadedBy(string uploadedBy)
    {
        return string.IsNullOrWhiteSpace(uploadedBy)
            ? "Lecturer"
            : uploadedBy.Trim();
    }

    private static string ExtractText(string filePath, string extension)
    {
        return extension switch
        {
            ".pdf" => ExtractPdfText(filePath),
            ".docx" => ExtractDocxText(filePath),
            _ => string.Empty
        };
    }

    private static string ExtractPdfText(string filePath)
    {
        var sb = new StringBuilder();

        using var document = PdfDocument.Open(filePath);
        foreach (var page in document.GetPages())
        {
            var text = ContentOrderTextExtractor.GetText(page);
            if (!string.IsNullOrWhiteSpace(text))
            {
                sb.AppendLine(text);
            }
        }

        return NormalizeExtractedText(sb.ToString());
    }

    private static string ExtractDocxText(string filePath)
    {
        var sb = new StringBuilder();

        using var document = WordprocessingDocument.Open(filePath, false);
        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
        {
            return string.Empty;
        }

        foreach (var paragraph in body.Descendants<OpenXmlParagraph>())
        {
            var paragraphText = string.Concat(paragraph.Descendants<OpenXmlText>().Select(text => text.Text));
            if (!string.IsNullOrWhiteSpace(paragraphText))
            {
                sb.AppendLine(paragraphText);
            }
        }

        return NormalizeExtractedText(sb.ToString());
    }

    private static string NormalizeExtractedText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        // PostgreSQL text columns reject NUL bytes (0x00 / \0) extracted from certain PDF/DOCX encodings
        var sanitized = text.Replace("\0", string.Empty).Replace("\u0000", string.Empty);

        var lines = sanitized
            .Replace("\r", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line));

        return string.Join(Environment.NewLine, lines);
    }

    private static List<string> ChunkText(string text)
    {
        const int wordsPerChunk = 350;

        var words = text
            .Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var chunks = new List<string>();
        for (var index = 0; index < words.Count; index += wordsPerChunk)
        {
            chunks.Add(string.Join(' ', words.Skip(index).Take(wordsPerChunk)));
        }

        return chunks;
    }

    private static string FormatEmbeddingPreview(Vector? embedding)
    {
        if (embedding == null)
        {
            return string.Empty;
        }

        var values = embedding
            .ToArray()
            .Take(8)
            .Select(value => value.ToString("0.0000", CultureInfo.InvariantCulture));

        return string.Join(",", values);
    }

    public async Task<List<Course>> GetAvailableCoursesForUserAsync(string userId, bool isAdmin)
    {
        if (isAdmin)
        {
            return await _courseRepository.GetAllAsync();
        }

        return await _courseRepository.GetAssignedCoursesAsync(userId);
    }

    public async Task<Document?> GetApprovedDocumentForStudentAsync(int id)
    {
        var document = await _documentRepository.GetByIdAsync(id, uploadedById: null);
        if (document != null && document.Status == DocumentStatuses.Approved)
        {
            return document;
        }
        return null;
    }

    public async Task<DocumentDeleteImpactDto> GetDeleteImpactAsync(int id, string currentUserId, bool isAdmin)
    {
        var ownerFilter = isAdmin ? null : currentUserId;
        var document = await _documentRepository.GetByIdAsync(id, ownerFilter);
        if (document == null)
        {
            throw new UnauthorizedAccessException("Tài liệu không tồn tại hoặc bạn không có quyền truy cập.");
        }

        var totalQuizzes = await _quizRepository.GetQuizzesCountByDocumentIdAsync(id);
        var activeQuizzes = await _quizRepository.GetQuizzesCountByDocumentIdAsync(id, QuizStatuses.Published);
        var totalAttempts = await _quizRepository.GetStudentAttemptsCountByDocumentIdAsync(id);

        var canHardDelete = totalAttempts == 0 && activeQuizzes == 0;
        var recommendedAction = canHardDelete ? DocumentDeleteActions.HardDelete : DocumentDeleteActions.Archive;

        string warningMessage = string.Empty;
        if (totalAttempts > 0)
        {
            warningMessage = $"Tài liệu này đã được sử dụng và có {totalAttempts} lượt làm bài của sinh viên. Không thể xóa để bảo toàn lịch sử học tập.";
        }
        else if (activeQuizzes > 0)
        {
            warningMessage = $"Tài liệu này đang liên kết với {activeQuizzes} bài Quiz đang hoạt động (Published). Không thể xóa trực tiếp.";
        }
        else if (totalQuizzes > 0)
        {
            warningMessage = $"Tài liệu này đang có {totalQuizzes} bài Quiz nháp (Draft) chưa có lượt làm bài. Xóa tài liệu sẽ xóa cả bài Quiz nháp này.";
        }

        return new DocumentDeleteImpactDto
        {
            DocumentId = id,
            FileName = document.FileName,
            TotalQuizzes = totalQuizzes,
            ActiveQuizzes = activeQuizzes,
            TotalAttempts = totalAttempts,
            RecommendedAction = recommendedAction,
            WarningMessage = warningMessage
        };
    }

    public async Task<DocumentDeleteResultDto> ExecuteDeleteOrArchiveAsync(int id, string action, string currentUserId, bool isAdmin)
    {
        if (action != DocumentDeleteActions.HardDelete && action != DocumentDeleteActions.Archive)
        {
            return new DocumentDeleteResultDto
            {
                IsSuccess = false,
                Message = "Hành động không hợp lệ.",
                ExecutedAction = action
            };
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var ownerFilter = isAdmin ? null : currentUserId;
            var document = await _documentRepository.GetByIdAsync(id, ownerFilter);
            if (document == null)
            {
                return new DocumentDeleteResultDto
                {
                    IsSuccess = false,
                    Message = "Tài liệu không tồn tại hoặc bạn không có quyền thực hiện.",
                    ExecutedAction = action
                };
            }

            var totalQuizzes = await _quizRepository.GetQuizzesCountByDocumentIdAsync(id);
            var activeQuizzes = await _quizRepository.GetQuizzesCountByDocumentIdAsync(id, QuizStatuses.Published);
            var totalAttempts = await _quizRepository.GetStudentAttemptsCountByDocumentIdAsync(id);

            var actualCanHardDelete = totalAttempts == 0 && activeQuizzes == 0;

            var finalAction = action;
            if (action == DocumentDeleteActions.HardDelete && !actualCanHardDelete)
            {
                finalAction = DocumentDeleteActions.Archive;
            }

            if (finalAction == DocumentDeleteActions.HardDelete)
            {
                var quizzes = await _quizRepository.GetQuizzesByDocumentIdAsync(id);
                if (quizzes.Any())
                {
                    var quizIds = quizzes.Select(q => q.Id).ToList();
                    await _quizRepository.DeleteQuizzesRangeAsync(quizIds);
                }

                await _documentRepository.DeleteAsync(document);
                await transaction.CommitAsync();

                if (!string.IsNullOrWhiteSpace(document.FilePath) &&
                    (document.FilePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                     document.FilePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) &&
                    !string.IsNullOrWhiteSpace(document.StoredFileName))
                {
                    try
                    {
                        await _cloudStorageService.DeleteFileAsync(document.StoredFileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Database deleted successfully, but failed to clean up Cloudinary file: {PublicId}", document.StoredFileName);
                        return new DocumentDeleteResultDto
                        {
                            IsSuccess = true,
                            Message = "Tài liệu đã được xóa khỏi hệ thống, nhưng không thể xóa tệp tin trên dịch vụ lưu trữ đám mây.",
                            ExecutedAction = DocumentDeleteActions.HardDelete
                        };
                    }
                }

                return new DocumentDeleteResultDto
                {
                    IsSuccess = true,
                    Message = "Xóa tài liệu và các dữ liệu liên quan thành công.",
                    ExecutedAction = DocumentDeleteActions.HardDelete
                };
            }
            else
            {
                document.Status = DocumentStatuses.Archived;
                await _documentRepository.UpdateAsync(document);

                var quizzes = await _quizRepository.GetQuizzesByDocumentIdAsync(id);
                foreach (var quiz in quizzes)
                {
                    quiz.Status = QuizStatuses.Archived;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new DocumentDeleteResultDto
                {
                    IsSuccess = true,
                    Message = "Lưu trữ tài liệu và các bài Quiz liên quan thành công.",
                    ExecutedAction = DocumentDeleteActions.Archive
                };
            }
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Lỗi xảy ra trong quá trình xóa/lưu trữ tài liệu ID = {Id}", id);
            return new DocumentDeleteResultDto
            {
                IsSuccess = false,
                Message = $"Có lỗi xảy ra: {ex.Message}",
                ExecutedAction = action
            };
        }
    }
}
