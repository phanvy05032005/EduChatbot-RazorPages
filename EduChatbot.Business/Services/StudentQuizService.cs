using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduChatbot.Data;
using EduChatbot.Data.Repositories;
using EduChatbot.Models;
using Microsoft.EntityFrameworkCore;

namespace EduChatbot.Business.Services;

public class StudentQuizService : IStudentQuizService
{
    private readonly IQuizRepository _quizRepository;
    private readonly ApplicationDbContext _context;
    private readonly ISubscriptionAccessService _subscriptionAccessService;

    public StudentQuizService(IQuizRepository quizRepository, ApplicationDbContext context, ISubscriptionAccessService subscriptionAccessService)
    {
        _quizRepository = quizRepository;
        _context = context;
        _subscriptionAccessService = subscriptionAccessService;
    }

    private async Task EnsurePremiumQuizAccessAsync(string studentId)
    {
        if (!await _subscriptionAccessService.CheckCanUseQuizAsync(studentId))
        {
            throw new InvalidOperationException("Premium subscription required for quizzes.");
        }
    }

    public async Task<List<Quiz>> GetAvailableQuizzesAsync(string studentId)
    {
        return await _quizRepository.GetAvailablePublishedQuizzesAsync(studentId);
    }

    public async Task<QuizAttempt> StartQuizAsync(int quizId, string studentId)
    {
        await EnsurePremiumQuizAccessAsync(studentId);
        var quiz = await _quizRepository.GetQuizWithQuestionsAndOptionsAsync(quizId);
        if (quiz == null || quiz.Status != QuizStatuses.Published)
        {
            throw new KeyNotFoundException("Quiz not found or not published.");
        }

        // 1. Check if there is an InProgress attempt
        var inProgressAttempt = await _quizRepository.GetInProgressAttemptAsync(quizId, studentId);
        if (inProgressAttempt != null)
        {
            return inProgressAttempt;
        }

        // 2. Check maximum attempts limit
        int submittedCount = await _quizRepository.CountSubmittedAttemptsAsync(quizId, studentId);
        if (submittedCount >= quiz.MaxAttempts)
        {
            throw new InvalidOperationException("You have reached the maximum number of attempts allowed for this quiz.");
        }

        // 3. Create new attempt
        var attempt = new QuizAttempt
        {
            QuizId = quizId,
            StudentId = studentId,
            Status = "InProgress",
            StartedAt = DateTime.UtcNow,
            TotalQuestions = quiz.Questions.Count
        };

        await _quizRepository.AddAttemptAsync(attempt);
        await _quizRepository.SaveChangesAsync();

        return attempt;
    }

    public async Task<StudentTakeQuizViewModel> GetTakeQuizAsync(int attemptId, string studentId)
    {
        await EnsurePremiumQuizAccessAsync(studentId);
        var attempt = await _quizRepository.GetAttemptForStudentAsync(attemptId, studentId);
        if (attempt == null)
        {
            throw new KeyNotFoundException("Attempt not found.");
        }

        if (attempt.Status != "InProgress")
        {
            throw new InvalidOperationException("This attempt is no longer in progress.");
        }

        var quiz = await _quizRepository.GetQuizWithQuestionsAndOptionsAsync(attempt.QuizId);
        if (quiz == null)
        {
            throw new KeyNotFoundException("Quiz not found.");
        }

        var deadline = attempt.StartedAt.AddMinutes(quiz.TimeLimitMinutes);

        var viewModel = new StudentTakeQuizViewModel
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            StartedAt = attempt.StartedAt,
            DeadlineAt = deadline,
            Questions = quiz.Questions.Select(q => new TakeQuestionDto
            {
                QuestionId = q.Id,
                QuestionText = q.QuestionText,
                QuestionOrder = q.QuestionOrder,
                Options = q.Options.Select(o => new TakeOptionDto
                {
                    OptionId = o.Id,
                    OptionText = o.OptionText,
                    OptionOrder = o.OptionOrder,
                    Label = o.Label
                }).ToList()
            }).ToList()
        };

        return viewModel;
    }

    public async Task<QuizAttempt> SubmitQuizAsync(int attemptId, string studentId, StudentSubmitQuizInput input)
    {
        await EnsurePremiumQuizAccessAsync(studentId);
        var attempt = await _quizRepository.GetAttemptForStudentAsync(attemptId, studentId);
        if (attempt == null)
        {
            throw new KeyNotFoundException("Attempt not found.");
        }

        if (attempt.StudentId != studentId)
        {
            throw new UnauthorizedAccessException("You cannot submit an attempt for another student.");
        }

        if (attempt.Status != "InProgress")
        {
            throw new InvalidOperationException("This attempt has already been submitted.");
        }

        var quiz = await _quizRepository.GetQuizWithQuestionsAndOptionsAsync(attempt.QuizId);
        if (quiz == null)
        {
            throw new KeyNotFoundException("Quiz not found.");
        }

        // Verify if submission is past the deadline (+ 2 minutes grace period for network latency)
        bool isExpired = DateTime.UtcNow > attempt.StartedAt.AddMinutes(quiz.TimeLimitMinutes).AddMinutes(2);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            int correctCount = 0;
            var answers = new List<QuizAttemptAnswer>();

            foreach (var question in quiz.Questions)
            {
                var selection = input.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
                int? selectedOptionId = selection?.SelectedOptionId;

                // Validate that the option belongs to the question
                if (selectedOptionId.HasValue)
                {
                    var validOption = question.Options.FirstOrDefault(o => o.Id == selectedOptionId.Value);
                    if (validOption == null)
                    {
                        throw new ArgumentException($"Selected option {selectedOptionId.Value} is invalid for question {question.Id}.");
                    }
                }

                bool isCorrect = false;
                if (selectedOptionId.HasValue)
                {
                    var option = question.Options.First(o => o.Id == selectedOptionId.Value);
                    isCorrect = option.IsCorrect;
                }

                if (isCorrect)
                {
                    correctCount++;
                }

                var attemptAnswer = new QuizAttemptAnswer
                {
                    QuizAttemptId = attempt.Id,
                    QuizQuestionId = question.Id,
                    SelectedOptionId = selectedOptionId,
                    IsCorrect = isCorrect
                };

                _context.QuizAttemptAnswers.Add(attemptAnswer);
                answers.Add(attemptAnswer);
            }

            double score = (double)correctCount * 100.0 / quiz.Questions.Count;

            attempt.Status = "Submitted";
            attempt.SubmittedAt = DateTime.UtcNow;
            attempt.CorrectCount = correctCount;
            attempt.Score = Math.Round(score, 2);

            await _quizRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            return attempt;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("Error during quiz submission: " + ex.Message);
        }
    }

    public async Task<StudentQuizResultViewModel> GetResultAsync(int attemptId, string studentId)
    {
        var attempt = await _quizRepository.GetAttemptWithDetailsAsync(attemptId, studentId);
        if (attempt == null)
        {
            throw new KeyNotFoundException("Attempt not found.");
        }

        if (attempt.Status == "InProgress" && !await _subscriptionAccessService.CheckCanUseQuizAsync(studentId))
        {
            throw new InvalidOperationException("Premium subscription required for in-progress quizzes.");
        }

        var quiz = attempt.Quiz;
        if (quiz == null)
        {
            throw new KeyNotFoundException("Quiz not found.");
        }

        var viewModel = new StudentQuizResultViewModel
        {
            AttemptId = attempt.Id,
            QuizId = quiz.Id,
            QuizTitle = quiz.Title,
            Status = attempt.Status,
            Score = attempt.Score ?? 0,
            CorrectCount = attempt.CorrectCount ?? 0,
            TotalQuestions = attempt.TotalQuestions,
            StartedAt = attempt.StartedAt,
            SubmittedAt = attempt.SubmittedAt,
            Questions = quiz.Questions.Select(q =>
            {
                var ans = attempt.Answers.FirstOrDefault(a => a.QuizQuestionId == q.Id);
                var correctOpt = q.Options.FirstOrDefault(o => o.IsCorrect);

                return new ResultQuestionDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    QuestionOrder = q.QuestionOrder,
                    Explanation = q.Explanation,
                    IsCorrect = ans?.IsCorrect ?? false,
                    SelectedOptionId = ans?.SelectedOptionId,
                    CorrectOptionId = correctOpt?.Id ?? 0,
                    Options = q.Options.Select(o => new ResultOptionDto
                    {
                        OptionId = o.Id,
                        OptionText = o.OptionText,
                        OptionOrder = o.OptionOrder,
                        Label = o.Label,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                };
            }).ToList()
        };

        return viewModel;
    }

    public async Task<List<StudentQuizHistoryItemViewModel>> GetHistoryAsync(string studentId)
    {
        var attempts = await _context.QuizAttempts
            .Include(a => a.Quiz)
                .ThenInclude(q => q!.Course)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();

        return attempts.Select(a => new StudentQuizHistoryItemViewModel
        {
            AttemptId = a.Id,
            QuizId = a.QuizId,
            QuizTitle = a.Quiz?.Title ?? "Unknown Quiz",
            CourseCode = a.Quiz?.Course?.Code ?? "N/A",
            CourseName = a.Quiz?.Course?.Name ?? "N/A",
            StartedAt = a.StartedAt,
            SubmittedAt = a.SubmittedAt,
            Status = a.Status,
            Score = a.Score,
            CorrectCount = a.CorrectCount,
            TotalQuestions = a.TotalQuestions
        }).ToList();
    }

    public async Task<int> GetSubmittedAttemptsCountAsync(int quizId, string studentId)
    {
        return await _quizRepository.CountSubmittedAttemptsAsync(quizId, studentId);
    }

    public async Task<QuizAttempt?> GetInProgressAttemptAsync(int quizId, string studentId)
    {
        return await _quizRepository.GetInProgressAttemptAsync(quizId, studentId);
    }
}
