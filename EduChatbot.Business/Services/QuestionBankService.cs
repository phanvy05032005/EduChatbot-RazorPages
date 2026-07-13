using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EduChatbot.Data;
using EduChatbot.Data.Repositories;
using EduChatbot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduChatbot.Business.Services;

public class QuestionBankService : IQuestionBankService
{
    private readonly IQuestionBankRepository _repository;
    private readonly ICourseRepository _courseRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<QuestionBankService> _logger;

    public QuestionBankService(
        IQuestionBankRepository repository,
        ICourseRepository courseRepository,
        ApplicationDbContext context,
        ILogger<QuestionBankService> logger)
    {
        _repository = repository;
        _courseRepository = courseRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<QuestionBankItemDto>> GetQuestionsAsync(QuestionBankFilterDto filter, string lecturerId, bool isAdmin)
    {
        var (items, totalCount) = await _repository.SearchQuestionsAsync(filter, lecturerId, isAdmin);

        var dtos = items.Select(item => new QuestionBankItemDto
        {
            Id = item.Id,
            CourseId = item.CourseId,
            CourseCode = item.Course?.Code ?? string.Empty,
            DocumentId = item.DocumentId,
            DocumentName = item.Document?.FileName ?? string.Empty,
            SourceChunkId = item.SourceChunkId,
            QuestionText = item.QuestionText,
            Explanation = item.Explanation,
            Difficulty = item.Difficulty,
            Status = item.Status,
            SourceType = item.SourceType,
            QuestionType = item.QuestionType,
            CreatedByLecturerId = item.CreatedByLecturerId,
            CreatedAt = item.CreatedAt,
            Tags = item.Tags
        }).ToList();

        return new PagedResult<QuestionBankItemDto>
        {
            Items = dtos,
            Page = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<QuestionBankItemDto?> GetQuestionByIdAsync(int id, string lecturerId, bool isAdmin)
    {
        var item = await _repository.GetByIdWithOptionsAsync(id);
        if (item == null) return null;

        // Check course access if not Admin
        if (!isAdmin)
        {
            var hasAccess = await _courseRepository.IsLecturerAssignedToCourseAsync(lecturerId, item.CourseId);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("You are not assigned to the course of this question.");
            }
        }

        return new QuestionBankItemDto
        {
            Id = item.Id,
            CourseId = item.CourseId,
            CourseCode = item.Course?.Code ?? string.Empty,
            DocumentId = item.DocumentId,
            DocumentName = item.Document?.FileName ?? string.Empty,
            SourceChunkId = item.SourceChunkId,
            QuestionText = item.QuestionText,
            Explanation = item.Explanation,
            Difficulty = item.Difficulty,
            Status = item.Status,
            SourceType = item.SourceType,
            QuestionType = item.QuestionType,
            CreatedByLecturerId = item.CreatedByLecturerId,
            CreatedAt = item.CreatedAt,
            Tags = item.Tags,
            Options = item.Options.Select(o => new QuestionBankOptionDto
            {
                Id = o.Id,
                OptionText = o.OptionText,
                Label = o.Label,
                OptionOrder = o.OptionOrder,
                IsCorrect = o.IsCorrect
            }).ToList()
        };
    }

    public async Task<QuestionBankItemDto> CreateQuestionAsync(CreateQuestionBankItemDto dto, string lecturerId)
    {
        var hasAccess = await _courseRepository.IsLecturerAssignedToCourseAsync(lecturerId, dto.CourseId);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("You are not assigned to this course.");
        }

        if (dto.Options.Count != 4)
        {
            throw new ArgumentException("Each question must have exactly 4 options.");
        }

        if (dto.Options.Count(o => o.IsCorrect) != 1)
        {
            throw new ArgumentException("Each question must have exactly 1 correct option.");
        }

        var hash = ComputeNormalizedHash(dto.QuestionText);
        var isDuplicate = await _repository.ExistsActiveDuplicateAsync(dto.CourseId, hash);
        if (isDuplicate)
        {
            throw new InvalidOperationException("Câu hỏi này đã tồn tại trong Ngân hàng câu hỏi của môn học này.");
        }

        var item = new QuestionBankItem
        {
            CourseId = dto.CourseId,
            DocumentId = dto.DocumentId,
            SourceChunkId = dto.SourceChunkId,
            QuestionText = dto.QuestionText.Trim(),
            QuestionTextHash = hash,
            Explanation = dto.Explanation?.Trim() ?? string.Empty,
            Difficulty = dto.Difficulty,
            Status = dto.Status,
            SourceType = "Manual",
            QuestionType = dto.QuestionType,
            CreatedByLecturerId = lecturerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Tags = dto.Tags?.Trim() ?? string.Empty
        };

        foreach (var optDto in dto.Options)
        {
            item.Options.Add(new QuestionBankOption
            {
                OptionText = optDto.OptionText.Trim(),
                Label = optDto.Label,
                OptionOrder = optDto.OptionOrder,
                IsCorrect = optDto.IsCorrect
            });
        }

        await _repository.AddAsync(item);
        await _repository.SaveChangesAsync();

        return new QuestionBankItemDto
        {
            Id = item.Id,
            CourseId = item.CourseId,
            DocumentId = item.DocumentId,
            SourceChunkId = item.SourceChunkId,
            QuestionText = item.QuestionText,
            Explanation = item.Explanation,
            Difficulty = item.Difficulty,
            Status = item.Status,
            SourceType = item.SourceType,
            QuestionType = item.QuestionType,
            CreatedByLecturerId = item.CreatedByLecturerId,
            CreatedAt = item.CreatedAt,
            Tags = item.Tags,
            Options = item.Options.Select(o => new QuestionBankOptionDto
            {
                Id = o.Id,
                OptionText = o.OptionText,
                Label = o.Label,
                OptionOrder = o.OptionOrder,
                IsCorrect = o.IsCorrect
            }).ToList()
        };
    }

    public async Task UpdateQuestionAsync(UpdateQuestionBankItemDto dto, string lecturerId, bool isAdmin)
    {
        var item = await _repository.GetByIdWithOptionsAsync(dto.Id);
        if (item == null)
        {
            throw new KeyNotFoundException("Question not found.");
        }

        if (!isAdmin)
        {
            var hasAccess = await _courseRepository.IsLecturerAssignedToCourseAsync(lecturerId, item.CourseId);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("You do not have permission to modify this question.");
            }
        }

        if (item.Status == "Archived")
        {
            throw new InvalidOperationException("Cannot update an Archived question.");
        }

        if (dto.Options.Count != 4)
        {
            throw new ArgumentException("Each question must have exactly 4 options.");
        }

        if (dto.Options.Count(o => o.IsCorrect) != 1)
        {
            throw new ArgumentException("Each question must have exactly 1 correct option.");
        }

        var newHash = ComputeNormalizedHash(dto.QuestionText);
        if (item.QuestionTextHash != newHash)
        {
            var isDuplicate = await _repository.ExistsActiveDuplicateAsync(item.CourseId, newHash);
            if (isDuplicate)
            {
                throw new InvalidOperationException("Câu hỏi mới bị trùng lặp với một câu hỏi hoạt động khác.");
            }
            item.QuestionTextHash = newHash;
        }

        item.QuestionText = dto.QuestionText.Trim();
        item.Explanation = dto.Explanation?.Trim() ?? string.Empty;
        item.Difficulty = dto.Difficulty;
        item.Status = dto.Status;
        item.Tags = dto.Tags?.Trim() ?? string.Empty;
        item.UpdatedAt = DateTime.UtcNow;

        // Update options
        for (int i = 0; i < 4; i++)
        {
            var optDto = dto.Options[i];
            var option = item.Options.FirstOrDefault(o => o.Id == optDto.Id);
            if (option != null)
            {
                option.OptionText = optDto.OptionText.Trim();
                option.IsCorrect = optDto.IsCorrect;
            }
        }

        await _repository.SaveChangesAsync();
    }

    public async Task<bool> DeleteOrArchiveQuestionAsync(int id, string lecturerId, bool isAdmin)
    {
        var item = await _repository.GetByIdAsync(id);
        if (item == null) return false;

        if (!isAdmin)
        {
            var hasAccess = await _courseRepository.IsLecturerAssignedToCourseAsync(lecturerId, item.CourseId);
            if (!hasAccess)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this question.");
            }
        }

        var isUsed = await _repository.IsUsedInQuizzesAsync(id);
        if (isUsed)
        {
            // Downgrade to Archive
            item.Status = "Archived";
            item.UpdatedAt = DateTime.UtcNow;
            await _repository.SaveChangesAsync();
            return true; // Archived instead of hard-deleted
        }
        else
        {
            // Hard delete
            await _repository.DeleteAsync(item);
            await _repository.SaveChangesAsync();
            return false; // Hard-deleted
        }
    }

    public async Task<(int SavedCount, int SkippedCount)> SaveToBankFromQuizQuestionsAsync(List<int> quizQuestionIds, string lecturerId)
    {
        if (quizQuestionIds == null || !quizQuestionIds.Any()) return (0, 0);

        var questions = await _context.QuizQuestions
            .Include(q => q.Quiz)
            .Include(q => q.Options)
            .Where(q => quizQuestionIds.Contains(q.Id))
            .ToListAsync();

        if (!questions.Any()) return (0, 0);

        // Check ownership/course assignment for each quiz
        foreach (var question in questions)
        {
            if (question.Quiz == null) continue;

            var courseId = question.Quiz.CourseId;
            var isAssigned = await _courseRepository.IsLecturerAssignedToCourseAsync(lecturerId, courseId);
            if (!isAssigned && question.Quiz.CreatedByLecturerId != lecturerId)
            {
                throw new UnauthorizedAccessException($"You do not have access to save questions from Quiz ID {question.Quiz.Id}.");
            }
        }

        int savedCount = 0;
        int skippedCount = 0;

        foreach (var qst in questions)
        {
            if (qst.Quiz == null) continue;

            var hash = ComputeNormalizedHash(qst.QuestionText);
            var isDuplicate = await _repository.ExistsActiveDuplicateAsync(qst.Quiz.CourseId, hash);
            if (isDuplicate)
            {
                // Skip duplicates in bulk imports
                _logger.LogInformation("Skipping duplicate question bank save for question text: {Text}", qst.QuestionText);
                skippedCount++;
                continue;
            }

            var item = new QuestionBankItem
            {
                CourseId = qst.Quiz.CourseId,
                DocumentId = qst.Quiz.DocumentId,
                SourceChunkId = qst.SourceChunkId,
                QuestionText = qst.QuestionText.Trim(),
                QuestionTextHash = hash,
                Explanation = qst.Explanation?.Trim() ?? string.Empty,
                Difficulty = qst.Quiz.Difficulty,
                Status = "Approved", // Lecturers reviewed it already before importing
                SourceType = "AI",
                QuestionType = "MultipleChoice",
                CreatedByLecturerId = lecturerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            foreach (var opt in qst.Options)
            {
                item.Options.Add(new QuestionBankOption
                {
                    OptionText = opt.OptionText.Trim(),
                    Label = opt.Label,
                    OptionOrder = opt.OptionOrder,
                    IsCorrect = opt.IsCorrect
                });
            }

            await _repository.AddAsync(item);
            savedCount++;
        }

        await _repository.SaveChangesAsync();
        return (savedCount, skippedCount);
    }

    public async Task<CreateQuizFromBankResultDto> CreateQuizFromBankAsync(CreateQuizFromBankDto dto, string lecturerId)
    {
        var hasAccess = await _courseRepository.IsLecturerAssignedToCourseAsync(lecturerId, dto.CourseId);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("You are not assigned to this course.");
        }

        if (dto.SelectedQuestionIds == null || !dto.SelectedQuestionIds.Any())
        {
            return new CreateQuizFromBankResultDto
            {
                IsSuccess = false,
                Message = "No questions were selected to create the quiz."
            };
        }

        var bankQuestions = await _repository.GetSelectedItemsAsync(dto.SelectedQuestionIds, dto.CourseId);
        if (!bankQuestions.Any())
        {
            return new CreateQuizFromBankResultDto
            {
                IsSuccess = false,
                Message = "None of the selected questions were found or they are archived."
            };
        }

        // Transactions managed inside the Service layer
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var quiz = new Quiz
            {
                CourseId = dto.CourseId,
                DocumentId = bankQuestions.First().DocumentId ?? 0, // Fallback placeholder or first document
                Title = dto.Title.Trim(),
                Difficulty = dto.Difficulty,
                AdditionalInstruction = dto.AdditionalInstruction?.Trim() ?? string.Empty,
                NumberOfQuestions = bankQuestions.Count,
                TimeLimitMinutes = dto.TimeLimitMinutes,
                MaxAttempts = dto.MaxAttempts,
                Status = "Draft", // Always created in Draft status
                CreatedByLecturerId = lecturerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // If there's no valid document linked, look for first ready document in course as EF requires it
            if (quiz.DocumentId == 0)
            {
                var doc = await _context.Documents.FirstOrDefaultAsync(d => d.CourseId == dto.CourseId && d.Status == "Approved");
                if (doc != null)
                {
                    quiz.DocumentId = doc.Id;
                }
                else
                {
                    // Fallback to any document in the course or throw
                    var fallbackDoc = await _context.Documents.FirstOrDefaultAsync(d => d.CourseId == dto.CourseId);
                    if (fallbackDoc != null)
                    {
                        quiz.DocumentId = fallbackDoc.Id;
                    }
                    else
                    {
                        return new CreateQuizFromBankResultDto
                        {
                            IsSuccess = false,
                            Message = "To create a quiz, the course must have at least one document uploaded."
                        };
                    }
                }
            }

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync(); // Generate quiz.Id

            int questionOrder = 1;
            foreach (var bankQ in bankQuestions)
            {
                var question = new QuizQuestion
                {
                    QuizId = quiz.Id,
                    QuestionOrder = questionOrder++,
                    QuestionText = bankQ.QuestionText,
                    Explanation = bankQ.Explanation,
                    SourceChunkId = bankQ.SourceChunkId,
                    SourceQuestionBankItemId = bankQ.Id
                };

                _context.QuizQuestions.Add(question);
                await _context.SaveChangesAsync(); // Generate question.Id

                int optionOrder = 1;
                foreach (var bankOpt in bankQ.Options)
                {
                    var option = new QuizOption
                    {
                        QuizQuestionId = question.Id,
                        OptionOrder = optionOrder++,
                        Label = bankOpt.Label,
                        OptionText = bankOpt.OptionText,
                        IsCorrect = bankOpt.IsCorrect
                    };
                    _context.QuizOptions.Add(option);
                }
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            return new CreateQuizFromBankResultDto
            {
                IsSuccess = true,
                Message = "Quiz created successfully from Question Bank.",
                QuizId = quiz.Id
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to create quiz from bank questions.");
            return new CreateQuizFromBankResultDto
            {
                IsSuccess = false,
                Message = $"Failed to create quiz: {ex.Message}"
            };
        }
    }

    private static string ComputeNormalizedHash(string text)
    {
        return QuestionTextNormalizer.ComputeHash(text);
    }

    public async Task CleanupSeededDataAsync()
    {
        // 1. Delete test question bank items matching specific text patterns
        var testQuestionTexts = new[] { 
            "Câu hỏi mẫu ngân hàng", 
            "Câu hỏi thêm vào đề review", 
            "Câu hỏi thuộc môn khác", 
            "Nội dung câu hỏi số 1", 
            "NỘI DUNG CÂU HỎI SỐ 1", 
            "Đã sửa đổi nội dung gốc trong bank", 
            "Nội dung câu hỏi số 2" 
        };

        var qbItems = await _context.QuestionBankItems
            .Where(q => testQuestionTexts.Any(pattern => q.QuestionText.Contains(pattern)))
            .ToListAsync();
        
        var qbItemIds = qbItems.Select(q => q.Id).ToList();

        // 2. Delete test courses
        var testCourseCodes = new List<string> { "TEST101", "TEST202", "TEST303", "OTHER303", "OTHER404" };
        var courses = await _context.Courses.Where(c => testCourseCodes.Contains(c.Code)).ToListAsync();
        var courseIds = courses.Select(c => c.Id).ToList();

        // Also retrieve items belonging to those test courses
        if (courseIds.Any())
        {
            var courseQbItems = await _context.QuestionBankItems.Where(q => courseIds.Contains(q.CourseId)).ToListAsync();
            foreach (var item in courseQbItems)
            {
                if (!qbItemIds.Contains(item.Id))
                {
                    qbItems.Add(item);
                    qbItemIds.Add(item.Id);
                }
            }
        }

        // Delete question bank options
        if (qbItemIds.Any())
        {
            var options = await _context.QuestionBankOptions.Where(o => qbItemIds.Contains(o.QuestionBankItemId)).ToListAsync();
            _context.QuestionBankOptions.RemoveRange(options);
            _context.QuestionBankItems.RemoveRange(qbItems);
        }

        // 3. Delete test quizzes
        var testQuizTitles = new[] { 
            "Đề thi từ ngân hàng", 
            "Đề nháp Review", 
            "Đề đã xuất bản", 
            "Đề thi nháp Phase 3", 
            "Đề thi đã xuất bản Phase 3" 
        };

        var quizzes = await _context.Quizzes
            .Where(q => testQuizTitles.Any(pattern => q.Title.Contains(pattern)) || courseIds.Contains(q.CourseId))
            .ToListAsync();
        
        var quizIds = quizzes.Select(q => q.Id).ToList();

        if (quizIds.Any())
        {
            var attempts = await _context.QuizAttempts.Where(qa => quizIds.Contains(qa.QuizId)).ToListAsync();
            _context.QuizAttempts.RemoveRange(attempts);

            var questions = await _context.QuizQuestions.Where(qq => quizIds.Contains(qq.QuizId)).ToListAsync();
            _context.QuizQuestions.RemoveRange(questions);

            _context.Quizzes.RemoveRange(quizzes);
        }

        // 4. Delete documents
        var docs = await _context.Documents
            .Where(d => d.FileName.Contains("scenario_test_doc") || d.FileName.Contains("phase3_test_doc") || (d.CourseId != null && courseIds.Contains(d.CourseId.Value)))
            .ToListAsync();
        _context.Documents.RemoveRange(docs);

        // 5. Delete course assignments
        if (courseIds.Any())
        {
            var lcAssignments = await _context.LecturerCourses.Where(lc => courseIds.Contains(lc.CourseId)).ToListAsync();
            _context.LecturerCourses.RemoveRange(lcAssignments);
            _context.Courses.RemoveRange(courses);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("SUCCESS: Seeded test data cleanup completed.");
    }
}
