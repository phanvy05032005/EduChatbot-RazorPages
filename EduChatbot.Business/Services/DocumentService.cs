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

    public DocumentService(
        IDocumentRepository documentRepository,
        ICourseRepository courseRepository,
        IDocumentUploadRules documentUploadRules,
        IEmbeddingService embeddingService,
        ILogger<DocumentService> logger,
        UserManager<ApplicationUser> userManager)
    {
        _documentRepository = documentRepository;
        _courseRepository = courseRepository;
        _documentUploadRules = documentUploadRules;
        _embeddingService = embeddingService;
        _logger = logger;
        _userManager = userManager;
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
        var safeOriginalFileName = Path.GetFileName(originalFileName)?.Trim() ?? string.Empty;
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

        string? physicalFilePath = null;
        try
        {
            var uploadFolder = Path.Combine(webRootPath, "uploads", "documents");
            Directory.CreateDirectory(uploadFolder);

            var extension = Path.GetExtension(safeOriginalFileName).ToLowerInvariant();
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            physicalFilePath = Path.Combine(uploadFolder, storedFileName);

            await using (var outputStream = File.Create(physicalFilePath))
            {
                await fileStream.CopyToAsync(outputStream);
            }

            var extractedText = ExtractText(physicalFilePath, extension);
            if (string.IsNullOrWhiteSpace(extractedText))
            {
                File.Delete(physicalFilePath);
                return new DocumentUploadResult
                {
                    IsSuccess = false,
                    Message = "Failed to extract text content from the file. Please check your PDF/DOCX file.",
                    Status = DocumentStatuses.Failed
                };
            }

            var documentChunks = await CreateDocumentChunksAsync(extractedText);

            var document = new Document
            {
                FileName = safeOriginalFileName,
                StoredFileName = storedFileName,
                FilePath = $"/uploads/documents/{storedFileName}",
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
                ValidationResult = null,
                Chunks = documentChunks
            };

            await _documentRepository.AddAsync(document);

            return new DocumentUploadResult
            {
                IsSuccess = true,
                Message = "Document uploaded and indexed successfully.",
                DocumentId = document.Id,
                ChunkCount = document.ChunkCount,
                Status = document.Status
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document upload/indexing failed for {FileName}", safeOriginalFileName);

            if (!string.IsNullOrWhiteSpace(physicalFilePath) && File.Exists(physicalFilePath))
            {
                File.Delete(physicalFilePath);
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

        await _documentRepository.DeleteAsync(document);

        var physicalFilePath = Path.Combine(webRootPath, document.FilePath.TrimStart('/'));
        if (File.Exists(physicalFilePath))
        {
            // Xóa file vật lý sau khi database đã xóa thành công.
            File.Delete(physicalFilePath);
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
        var lines = text
            .Replace("\r", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line));

        return string.Join(Environment.NewLine, lines);
    }

    private static List<string> ChunkText(string text)
    {
        const int wordsPerChunk = 220;

        var words = text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
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
}
