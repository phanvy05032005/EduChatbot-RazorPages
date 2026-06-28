using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EduChatbot.Data;
using EduChatbot.Data.Repositories;
using EduChatbot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduChatbot.Business.Services;

public class LecturerQuizService : ILecturerQuizService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _settings;
    private readonly ILogger<LecturerQuizService> _logger;

    public LecturerQuizService(
        IQuizRepository quizRepository,
        IDocumentRepository documentRepository,
        ICourseRepository courseRepository,
        ApplicationDbContext context,
        HttpClient httpClient,
        IOptions<OpenRouterSettings> settings,
        ILogger<LecturerQuizService> logger)
    {
        _quizRepository = quizRepository;
        _documentRepository = documentRepository;
        _courseRepository = courseRepository;
        _context = context;
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<List<Quiz>> GetLecturerQuizzesAsync(string lecturerId)
    {
        return await _quizRepository.GetQuizzesByLecturerAsync(lecturerId);
    }

    public async Task<List<Document>> GetReadyDocumentsAsync(string lecturerId)
    {
        var assignedCourses = await _courseRepository.GetAssignedCoursesAsync(lecturerId);
        var courseIds = assignedCourses.Select(c => c.Id).ToList();

        var allDocs = await _documentRepository.GetAllAsync();
        return allDocs
            .Where(d => d.Status == DocumentStatuses.Approved && d.CourseId.HasValue && courseIds.Contains(d.CourseId.Value))
            .ToList();
    }

    public async Task<Quiz> GenerateQuizDraftAsync(LecturerGenerateQuizInput input, string lecturerId)
    {
        // 1. Validate Course access
        var hasAccess = await _courseRepository.IsLecturerAssignedToCourseAsync(lecturerId, input.CourseId);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("You are not assigned to this course.");
        }

        // 2. Validate Document status and association
        var document = await _documentRepository.GetByIdAsync(input.DocumentId);
        if (document == null || document.CourseId != input.CourseId || document.Status != DocumentStatuses.Approved)
        {
            throw new ArgumentException("The selected document is invalid or not approved.");
        }

        // 3. Extract Context Text
        string docContext = document.ExtractedText;
        if (string.IsNullOrWhiteSpace(docContext) && document.Chunks.Count > 0)
        {
            docContext = string.Join("\n", document.Chunks.Select(c => c.Content));
        }

        if (string.IsNullOrWhiteSpace(docContext))
        {
            throw new ArgumentException("The document does not contain readable content to generate questions.");
        }

        // Limit docContext to prevent token overflow (~10k words)
        if (docContext.Length > 30000)
        {
            docContext = docContext.Substring(0, 30000);
        }

        // 4. Call AI to generate quiz content (with 1 retry)
        List<AiQuizQuestion> aiQuestions = await GenerateQuizQuestionsWithRetryAsync(input, docContext);

        if (aiQuestions == null || aiQuestions.Count == 0)
        {
            throw new Exception("AI failed to generate valid quiz questions. Please try again.");
        }

        // 5. Save Quiz & Questions in transaction
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var quiz = new Quiz
            {
                CourseId = input.CourseId,
                DocumentId = input.DocumentId,
                Title = input.Title,
                Difficulty = input.Difficulty,
                AdditionalInstruction = input.AdditionalInstruction,
                NumberOfQuestions = aiQuestions.Count,
                TimeLimitMinutes = input.TimeLimitMinutes,
                MaxAttempts = input.MaxAttempts,
                Status = QuizStatuses.Draft,
                CreatedByLecturerId = lecturerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _quizRepository.AddQuizAsync(quiz);
            await _quizRepository.SaveChangesAsync(); // Generates QuizId

            int questionOrder = 1;
            foreach (var aiQ in aiQuestions)
            {
                // Try linking to a source chunk if possible
                int? sourceChunkId = null;
                if (document.Chunks.Count > 0)
                {
                    // Associate round-robin or simple heuristic
                    sourceChunkId = document.Chunks[(questionOrder - 1) % document.Chunks.Count].Id;
                }

                var question = new QuizQuestion
                {
                    QuizId = quiz.Id,
                    QuestionOrder = questionOrder++,
                    QuestionText = aiQ.question_text,
                    Explanation = aiQ.explanation,
                    SourceChunkId = sourceChunkId
                };

                _context.QuizQuestions.Add(question);
                await _context.SaveChangesAsync(); // Generates QuestionId

                int optionOrder = 1;
                char[] labels = { 'A', 'B', 'C', 'D' };
                for (int i = 0; i < aiQ.options.Count; i++)
                {
                    var opt = aiQ.options[i];
                    var option = new QuizOption
                    {
                        QuizQuestionId = question.Id,
                        OptionOrder = optionOrder,
                        Label = labels[i % 4].ToString(),
                        OptionText = opt.option_text,
                        IsCorrect = opt.is_correct
                    };
                    _context.QuizOptions.Add(option);
                    optionOrder++;
                }
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return quiz;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to save generated quiz to database.");
            throw new Exception("Error saving generated quiz: " + ex.Message);
        }
    }

    public async Task<Quiz?> GetQuizForReviewAsync(int quizId, string lecturerId)
    {
        var quiz = await _quizRepository.GetQuizWithQuestionsAndOptionsAsync(quizId);
        if (quiz == null) return null;

        if (quiz.CreatedByLecturerId != lecturerId)
        {
            throw new UnauthorizedAccessException("You do not own this quiz.");
        }

        return quiz;
    }

    public async Task UpdateQuestionAsync(int quizId, string lecturerId, LecturerSaveQuestionInput input)
    {
        var quiz = await _quizRepository.GetQuizWithQuestionsAndOptionsAsync(quizId);
        if (quiz == null)
        {
            throw new KeyNotFoundException("Quiz not found.");
        }

        if (quiz.CreatedByLecturerId != lecturerId)
        {
            throw new UnauthorizedAccessException("Access denied.");
        }

        EnsureDraft(quiz, "update question");

        if (!input.QuestionId.HasValue)
        {
            throw new ArgumentException("Question ID is required for update.");
        }

        var question = quiz.Questions.FirstOrDefault(q => q.Id == input.QuestionId.Value);
        if (question == null)
        {
            throw new KeyNotFoundException("Question not found in this quiz.");
        }

        // Validate options
        if (input.Options.Count != 4)
        {
            throw new ArgumentException("Each question must have exactly 4 options.");
        }

        if (input.Options.Count(o => o.IsCorrect) != 1)
        {
            throw new ArgumentException("Each question must have exactly 1 correct option.");
        }

        // Update question content
        question.QuestionText = input.QuestionText;
        question.Explanation = input.Explanation;

        // Update options
        for (int i = 0; i < 4; i++)
        {
            var optInput = input.Options[i];
            var option = question.Options.FirstOrDefault(o => o.Id == optInput.OptionId);
            if (option != null)
            {
                option.OptionText = optInput.OptionText;
                option.IsCorrect = optInput.IsCorrect;
            }
        }

        quiz.UpdatedAt = DateTime.UtcNow;
        await _quizRepository.SaveChangesAsync();
    }

    public async Task DeleteQuestionAsync(int quizId, int questionId, string lecturerId)
    {
        var quiz = await _quizRepository.GetQuizWithQuestionsAndOptionsAsync(quizId);
        if (quiz == null)
        {
            throw new KeyNotFoundException("Quiz not found.");
        }

        if (quiz.CreatedByLecturerId != lecturerId)
        {
            throw new UnauthorizedAccessException("Access denied.");
        }

        EnsureDraft(quiz, "delete question");

        var question = quiz.Questions.FirstOrDefault(q => q.Id == questionId);
        if (question == null)
        {
            throw new KeyNotFoundException("Question not found.");
        }

        _context.QuizQuestions.Remove(question);

        // Re-order remaining questions
        quiz.Questions.Remove(question);
        int order = 1;
        foreach (var q in quiz.Questions.OrderBy(x => x.QuestionOrder))
        {
            q.QuestionOrder = order++;
        }

        quiz.NumberOfQuestions = quiz.Questions.Count;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _quizRepository.SaveChangesAsync();
    }

    public async Task PublishQuizAsync(int quizId, string lecturerId)
    {
        var quiz = await _quizRepository.GetQuizWithQuestionsAndOptionsAsync(quizId);
        if (quiz == null)
        {
            throw new KeyNotFoundException("Quiz not found.");
        }

        if (quiz.CreatedByLecturerId != lecturerId)
        {
            throw new UnauthorizedAccessException("Access denied.");
        }

        if (quiz.Status != QuizStatuses.Draft)
        {
            throw new InvalidOperationException("Only draft quizzes can be published.");
        }

        if (quiz.Questions.Count == 0)
        {
            throw new InvalidOperationException("Cannot publish an empty quiz.");
        }

        // Validate all questions have 4 options and exactly 1 correct
        foreach (var question in quiz.Questions)
        {
            if (string.IsNullOrWhiteSpace(question.QuestionText))
            {
                throw new InvalidOperationException("All questions must have valid text content.");
            }

            if (question.Options.Count != 4)
            {
                throw new InvalidOperationException($"Question '{question.QuestionText}' must have exactly 4 options.");
            }

            if (question.Options.Any(o => string.IsNullOrWhiteSpace(o.OptionText)))
            {
                throw new InvalidOperationException($"All options in question '{question.QuestionText}' must have valid text content.");
            }

            if (question.Options.Count(o => o.IsCorrect) != 1)
            {
                throw new InvalidOperationException($"Question '{question.QuestionText}' must have exactly 1 correct option.");
            }
        }

        quiz.Status = QuizStatuses.Published;
        quiz.PublishedAt = DateTime.UtcNow;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _quizRepository.SaveChangesAsync();
    }

    public async Task<List<QuizAttempt>> GetQuizAttemptsAsync(int quizId, string lecturerId)
    {
        var quiz = await _quizRepository.GetQuizWithQuestionsAndOptionsAsync(quizId);
        if (quiz == null)
        {
            throw new KeyNotFoundException("Quiz not found.");
        }

        if (quiz.CreatedByLecturerId != lecturerId)
        {
            throw new UnauthorizedAccessException("Access denied.");
        }

        return await _context.QuizAttempts
            .Include(a => a.Student)
            .Where(a => a.QuizId == quizId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();
    }

    // ==========================================
    // AI INTEGRATION HELPER METHODS
    // ==========================================

    private async Task<List<AiQuizQuestion>> GenerateQuizQuestionsWithRetryAsync(LecturerGenerateQuizInput input, string docContext)
    {
        int retries = 1;
        for (int attempt = 0; attempt <= retries; attempt++)
        {
            try
            {
                var questions = await CallLlmToGenerateQuizAsync(input, docContext);
                if (questions != null && questions.Count > 0)
                {
                    // Validate JSON requirements
                    bool allValid = true;
                    foreach (var q in questions)
                    {
                        if (string.IsNullOrWhiteSpace(q.question_text) || q.options.Count != 4 || q.options.Count(o => o.is_correct) != 1 || q.options.Any(o => string.IsNullOrWhiteSpace(o.option_text)))
                        {
                            allValid = false;
                            break;
                        }
                    }

                    if (allValid)
                    {
                        return questions;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"AI Generation attempt {attempt + 1} failed.");
                if (attempt == retries)
                {
                    throw;
                }
            }
        }

        return [];
    }

    private async Task<List<AiQuizQuestion>?> CallLlmToGenerateQuizAsync(LecturerGenerateQuizInput input, string docContext)
    {
        var systemPrompt = @"You are a strict education assistant. Your task is to generate a quiz in Vietnamese based on the provided document content.
You MUST output a raw JSON array matching this exact schema:
[
  {
    ""question_text"": ""Nội dung câu hỏi?"",
    ""explanation"": ""Giải thích vì sao chọn đáp án này..."",
    ""options"": [
      { ""option_text"": ""Phương án A"", ""is_correct"": false },
      { ""option_text"": ""Phương án B"", ""is_correct"": true },
      { ""option_text"": ""Phương án C"", ""is_correct"": false },
      { ""option_text"": ""Phương án D"", ""is_correct"": false }
    ]
  }
]

RÀNG BUỘC QUAN TRỌNG:
1. Số lượng câu hỏi: " + input.NumberOfQuestions + @"
2. Mỗi câu hỏi PHẢI có đúng 4 phương án lựa chọn.
3. Mỗi câu hỏi chỉ có ĐÚNG 1 phương án có ""is_correct"": true.
4. Nội dung câu hỏi phải bám sát tài liệu, không tự chế thông tin ngoài ngữ cảnh.
5. Định dạng đầu ra: TRẢ VỀ JSON THUẦN TÚY. KHÔNG BỌC TRONG BLOCK CODE ```json VÀ KHÔNG CHỨA BẤT KỲ ĐOẠN TEXT GIẢI THÍCH NÀO NGOÀI JSON.";

        var requestBody = new
        {
            model = _settings.Model,
            temperature = 0.3,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = "Nội dung tài liệu:\n" + docContext }
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl)
        {
            Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"AI service returned status code {(int)response.StatusCode}.");
        }

        using var jsonDoc = JsonDocument.Parse(responseBody);
        var rawContent = jsonDoc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return null;
        }

        string cleanedJson = CleanJsonResponse(rawContent);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<List<AiQuizQuestion>>(cleanedJson, options);
    }

    public async Task<QuizQuestion> AddQuestionAsync(int quizId, string lecturerId, LecturerSaveQuestionInput input)
    {
        var quiz = await _quizRepository.GetQuizWithQuestionsAndOptionsAsync(quizId);
        if (quiz == null)
        {
            throw new KeyNotFoundException("Quiz not found.");
        }

        if (quiz.CreatedByLecturerId != lecturerId)
        {
            throw new UnauthorizedAccessException("Access denied. You do not own this quiz.");
        }

        EnsureDraft(quiz, "add question");

        if (string.IsNullOrWhiteSpace(input.QuestionText))
        {
            throw new ArgumentException("Question text is required.");
        }

        if (input.Options == null || input.Options.Count != 4)
        {
            throw new ArgumentException("Each question must have exactly 4 options.");
        }

        if (input.Options.Any(o => string.IsNullOrWhiteSpace(o.OptionText)))
        {
            throw new ArgumentException("All options must have non-empty text content.");
        }

        if (input.Options.Count(o => o.IsCorrect) != 1)
        {
            throw new ArgumentException("Each question must have exactly 1 correct option.");
        }

        int nextOrder = 1;
        if (quiz.Questions.Count > 0)
        {
            nextOrder = quiz.Questions.Max(q => q.QuestionOrder) + 1;
        }

        int? sourceChunkId = null;
        var document = await _context.Documents
            .Include(d => d.Chunks)
            .FirstOrDefaultAsync(d => d.Id == quiz.DocumentId);
        if (document != null && document.Chunks.Count > 0)
        {
            sourceChunkId = document.Chunks[(nextOrder - 1) % document.Chunks.Count].Id;
        }

        var question = new QuizQuestion
        {
            QuizId = quiz.Id,
            QuestionOrder = nextOrder,
            QuestionText = input.QuestionText,
            Explanation = input.Explanation,
            SourceChunkId = sourceChunkId
        };

        _context.QuizQuestions.Add(question);
        await _context.SaveChangesAsync();

        int optionOrder = 1;
        char[] labels = { 'A', 'B', 'C', 'D' };
        for (int i = 0; i < 4; i++)
        {
            var optInput = input.Options[i];
            var option = new QuizOption
            {
                QuizQuestionId = question.Id,
                OptionOrder = optionOrder,
                Label = labels[i].ToString(),
                OptionText = optInput.OptionText,
                IsCorrect = optInput.IsCorrect
            };
            _context.QuizOptions.Add(option);
            optionOrder++;
        }
        await _context.SaveChangesAsync();

        quiz.Questions.Add(question);
        quiz.NumberOfQuestions = quiz.Questions.Count;
        quiz.UpdatedAt = DateTime.UtcNow;

        await _quizRepository.SaveChangesAsync();

        return question;
    }

    public async Task<List<QuizQuestion>> GenerateMoreQuestionsAsync(int quizId, string lecturerId, GenerateMoreQuestionsInput input)
    {
        var quiz = await _quizRepository.GetQuizWithQuestionsAndOptionsAsync(quizId);
        if (quiz == null)
        {
            throw new KeyNotFoundException("Quiz not found.");
        }

        if (quiz.CreatedByLecturerId != lecturerId)
        {
            throw new UnauthorizedAccessException("Access denied. You do not own this quiz.");
        }

        EnsureDraft(quiz, "generate more questions");

        if (input.AdditionalQuestionCount < 1 || input.AdditionalQuestionCount > 10)
        {
            throw new ArgumentException("Can only generate between 1 and 10 questions at a time.");
        }

        var document = await _context.Documents
            .Include(d => d.Chunks)
            .FirstOrDefaultAsync(d => d.Id == quiz.DocumentId);
        if (document == null)
        {
            throw new KeyNotFoundException("Source document not found.");
        }

        if (document.Status != DocumentStatuses.Approved)
        {
            throw new InvalidOperationException("Source document must be approved before generating questions.");
        }

        string docContext = document.ExtractedText ?? string.Empty;
        if (string.IsNullOrWhiteSpace(docContext) && document.Chunks.Count > 0)
        {
            docContext = string.Join("\n", document.Chunks.Select(c => c.Content));
        }

        if (docContext.Length > 30000)
        {
            docContext = docContext.Substring(0, 30000);
        }

        var existingQuestions = quiz.Questions.Select(q => q.QuestionText).ToList();

        List<AiQuizQuestion> aiQuestions = await GenerateMoreAiQuestionsWithRetryAsync(input.AdditionalQuestionCount, input.AdditionalInstruction, docContext, existingQuestions);

        if (aiQuestions == null || aiQuestions.Count == 0)
        {
            throw new Exception("AI failed to generate valid additional questions. Please try again.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            int nextOrder = 1;
            if (quiz.Questions.Count > 0)
            {
                nextOrder = quiz.Questions.Max(q => q.QuestionOrder) + 1;
            }

            var newQuestions = new List<QuizQuestion>();

            foreach (var aiQ in aiQuestions)
            {
                int? sourceChunkId = null;
                if (document.Chunks.Count > 0)
                {
                    sourceChunkId = document.Chunks[(nextOrder - 1) % document.Chunks.Count].Id;
                }

                var question = new QuizQuestion
                {
                    QuizId = quiz.Id,
                    QuestionOrder = nextOrder++,
                    QuestionText = aiQ.question_text,
                    Explanation = aiQ.explanation,
                    SourceChunkId = sourceChunkId
                };

                _context.QuizQuestions.Add(question);
                await _context.SaveChangesAsync();

                int optionOrder = 1;
                char[] labels = { 'A', 'B', 'C', 'D' };
                for (int i = 0; i < aiQ.options.Count; i++)
                {
                    var opt = aiQ.options[i];
                    var option = new QuizOption
                    {
                        QuizQuestionId = question.Id,
                        OptionOrder = optionOrder,
                        Label = labels[i % 4].ToString(),
                        OptionText = opt.option_text,
                        IsCorrect = opt.is_correct
                    };
                    _context.QuizOptions.Add(option);
                    optionOrder++;
                }
                await _context.SaveChangesAsync();

                newQuestions.Add(question);
                quiz.Questions.Add(question);
            }

            quiz.NumberOfQuestions = quiz.Questions.Count;
            quiz.UpdatedAt = DateTime.UtcNow;

            await _quizRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            return newQuestions;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to save additional generated questions to database.");
            throw new Exception("Error saving generated questions: " + ex.Message);
        }
    }

    private async Task<List<AiQuizQuestion>> GenerateMoreAiQuestionsWithRetryAsync(int count, string additionalInstruction, string docContext, List<string> existingQuestions)
    {
        int retries = 1;
        for (int attempt = 0; attempt <= retries; attempt++)
        {
            try
            {
                var questions = await CallLlmToGenerateMoreQuizAsync(count, additionalInstruction, docContext, existingQuestions);
                if (questions != null && questions.Count > 0)
                {
                    bool allValid = true;
                    foreach (var q in questions)
                    {
                        if (string.IsNullOrWhiteSpace(q.question_text) || q.options.Count != 4 || q.options.Count(o => o.is_correct) != 1 || q.options.Any(o => string.IsNullOrWhiteSpace(o.option_text)))
                        {
                            allValid = false;
                            break;
                        }
                    }

                    if (allValid)
                    {
                        return questions;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"AI Generate More attempt {attempt + 1} failed.");
                if (attempt == retries)
                {
                    throw;
                }
            }
        }

        return [];
    }

    private async Task<List<AiQuizQuestion>?> CallLlmToGenerateMoreQuizAsync(int count, string additionalInstruction, string docContext, List<string> existingQuestions)
    {
        var existingListText = string.Join("\n- ", existingQuestions);
        var systemPrompt = @"You are a strict education assistant. Your task is to generate more multiple-choice questions in Vietnamese based on the provided document content.
You MUST output a raw JSON array matching this exact schema:
[
  {
    ""question_text"": ""Nội dung câu hỏi?"",
    ""explanation"": ""Giải thích vì sao chọn đáp án này..."",
    ""options"": [
      { ""option_text"": ""Phương án A"", ""is_correct"": false },
      { ""option_text"": ""Phương án B"", ""is_correct"": true },
      { ""option_text"": ""Phương án C"", ""is_correct"": false },
      { ""option_text"": ""Phương án D"", ""is_correct"": false }
    ]
  }
]

RÀNG BUỘC QUAN TRỌNG:
1. Số lượng câu hỏi cần sinh thêm: " + count + @"
2. Mỗi câu hỏi PHẢI có đúng 4 phương án lựa chọn.
3. Mỗi câu hỏi chỉ có ĐÚNG 1 phương án có ""is_correct"": true.
4. Nội dung câu hỏi phải bám sát tài liệu.
5. TRÁNH TRÙNG LẶP hoặc tương tự với các câu hỏi đã có sẵn dưới đây:
" + (existingQuestions.Count > 0 ? "- " + existingListText : "(Chưa có câu hỏi nào)") + @"
6. Định dạng đầu ra: TRẢ VỀ JSON THUẦN TÚY. KHÔNG BỌC TRONG BLOCK CODE ```json VÀ KHÔNG CHỨA BẤT KỲ ĐOẠN TEXT GIẢI THÍCH NÀO NGOÀI JSON.";

        var requestBody = new
        {
            model = _settings.Model,
            temperature = 0.4,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = "Nội dung tài liệu:\n" + docContext + (string.IsNullOrWhiteSpace(additionalInstruction) ? "" : "\nYêu cầu bổ sung: " + additionalInstruction) }
            }
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl)
        {
            Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        var response = await _httpClient.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"AI service returned status code {(int)response.StatusCode}.");
        }

        using var jsonDoc = JsonDocument.Parse(responseBody);
        var rawContent = jsonDoc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return null;
        }

        string cleanedJson = CleanJsonResponse(rawContent);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<List<AiQuizQuestion>>(cleanedJson, options);
    }

    private string CleanJsonResponse(string rawResponse)
    {
        var cleaned = rawResponse.Trim();
        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(7).Trim();
        }
        else if (cleaned.StartsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(3).Trim();
        }
        if (cleaned.EndsWith("```"))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 3).Trim();
        }
        return cleaned;
    }

    public async Task ArchiveQuizAsync(int quizId, string lecturerId)
    {
        var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);
        if (quiz == null)
        {
            throw new KeyNotFoundException("Quiz not found.");
        }

        if (quiz.CreatedByLecturerId != lecturerId)
        {
            throw new UnauthorizedAccessException("Access denied. You do not own this quiz.");
        }

        if (quiz.Status == QuizStatuses.Draft)
        {
            throw new InvalidOperationException("Draft quiz should be edited or deleted, not archived.");
        }

        if (quiz.Status == QuizStatuses.Archived)
        {
            return; // Controlled pass
        }

        if (quiz.Status == QuizStatuses.Published)
        {
            quiz.Status = QuizStatuses.Archived;
            quiz.UpdatedAt = DateTime.UtcNow;
            await _quizRepository.SaveChangesAsync();
        }
    }

    private void EnsureDraft(Quiz quiz, string actionName)
    {
        if (quiz.Status != QuizStatuses.Draft)
        {
            throw new InvalidOperationException($"Cannot {actionName} because the quiz is {quiz.Status}. Only Draft quizzes can be modified.");
        }
    }

    private class AiQuizQuestion
    {
        public string question_text { get; set; } = string.Empty;
        public string explanation { get; set; } = string.Empty;
        public List<AiQuizOption> options { get; set; } = [];
    }

    private class AiQuizOption
    {
        public string option_text { get; set; } = string.Empty;
        public bool is_correct { get; set; }
    }
}
