using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using EduChatbot.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Lecturer.Quizzes;

[Authorize(Roles = ApplicationRoles.AdminAndLecturer)]
public class ReviewModel : PageModel
{
    private readonly ILecturerQuizService _lecturerQuizService;
    private readonly IStudentRealtimeNotifier _studentRealtimeNotifier; // SignalR optional
    private readonly IQuestionBankService _questionBankService;

    public ReviewModel(
        ILecturerQuizService lecturerQuizService, 
        IStudentRealtimeNotifier studentRealtimeNotifier,
        IQuestionBankService questionBankService)
    {
        _lecturerQuizService = lecturerQuizService;
        _studentRealtimeNotifier = studentRealtimeNotifier;
        _questionBankService = questionBankService;
    }

    public Quiz Quiz { get; set; } = null!;

    public List<QuestionBankItemDto> AvailableBankQuestions { get; set; } = [];

    public bool IsAlreadyAddedInQuiz(string questionText, int bankItemId)
    {
        if (Quiz == null) return false;
        var normalizedInput = QuestionTextNormalizer.Normalize(questionText);
        return Quiz.Questions.Any(q => 
            q.SourceQuestionBankItemId == bankItemId || 
            QuestionTextNormalizer.Normalize(q.QuestionText) == normalizedInput
        );
    }

    [BindProperty]
    public LecturerSaveQuestionInput SaveInput { get; set; } = new();

    [BindProperty]
    public LecturerSaveQuestionInput AddInput { get; set; } = new();

    [BindProperty]
    public GenerateMoreQuestionsInput GenerateMoreInput { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var quiz = await _lecturerQuizService.GetQuizForReviewAsync(id, lecturerId);
            if (quiz == null)
            {
                return NotFound("Quiz not found.");
            }
            Quiz = quiz;

            // Load approved question bank items for this course to display in modal
            var filter = new QuestionBankFilterDto
            {
                CourseId = quiz.CourseId,
                Status = "Approved",
                PageNumber = 1,
                PageSize = 200
            };
            var isAdmin = User.IsInRole(ApplicationRoles.Admin);
            var bankResult = await _questionBankService.GetQuestionsAsync(filter, lecturerId, isAdmin);
            AvailableBankQuestions = bankResult.Items.ToList();

            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    public async Task<IActionResult> OnPostUpdateQuestionAsync(int id)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _lecturerQuizService.UpdateQuestionAsync(id, lecturerId, SaveInput);
            TempData["SuccessMessage"] = "Question updated successfully.";
            return RedirectToPage(new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage(new { id });
        }
    }

    public async Task<IActionResult> OnPostDeleteQuestionAsync(int id, int questionId)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _lecturerQuizService.DeleteQuestionAsync(id, questionId, lecturerId);
            TempData["SuccessMessage"] = "Question deleted successfully.";
            return RedirectToPage(new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage(new { id });
        }
    }

    public async Task<IActionResult> OnPostPublishAsync(int id)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _lecturerQuizService.PublishQuizAsync(id, lecturerId);
            TempData["SuccessMessage"] = "Quiz published successfully. It is now visible to students.";

            // Optional: send SignalR notification
            try
            {
                var quiz = await _lecturerQuizService.GetQuizForReviewAsync(id, lecturerId);
                if (quiz != null)
                {
                    await _studentRealtimeNotifier.NotifyQuizPublishedAsync(new StudentQuizPublishedPayload
                    {
                        QuizId = quiz.Id,
                        CourseId = quiz.CourseId,
                        CourseCode = quiz.Course?.Code ?? string.Empty,
                        QuizTitle = quiz.Title
                    });
                }
            }
            catch
            {
                // Ignore notification failure
            }

            return RedirectToPage("/Lecturer/Quizzes/Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage(new { id });
        }
    }

    public async Task<IActionResult> OnPostAddQuestionAsync(int id)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _lecturerQuizService.AddQuestionAsync(id, lecturerId, AddInput);
            TempData["SuccessMessage"] = "Question added manually successfully.";
            return RedirectToPage(new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage(new { id });
        }
    }

    public async Task<IActionResult> OnPostGenerateMoreAsync(int id)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var questions = await _lecturerQuizService.GenerateMoreQuestionsAsync(id, lecturerId, GenerateMoreInput);
            TempData["SuccessMessage"] = $"Generated {questions.Count} more questions with AI successfully.";
            return RedirectToPage(new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage(new { id });
        }
    }

    public async Task<IActionResult> OnPostArchiveAsync(int id)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _lecturerQuizService.ArchiveQuizAsync(id, lecturerId);
            TempData["SuccessMessage"] = "Quiz archived successfully. Students will no longer see this quiz for new attempts, but existing attempts and results will be kept.";
            return RedirectToPage(new { id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage(new { id });
        }
    }

    public async Task<IActionResult> OnPostSaveToBankAsync(int id, List<int> selectedQuizQuestionIds)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (selectedQuizQuestionIds == null || !selectedQuizQuestionIds.Any())
        {
            TempData["ErrorMessage"] = "No questions selected to save to Question Bank.";
            return RedirectToPage(new { id });
        }

        try
        {
            var (savedCount, skippedCount) = await _questionBankService.SaveToBankFromQuizQuestionsAsync(selectedQuizQuestionIds, lecturerId);
            TempData["SuccessMessage"] = $"Lưu thành công {savedCount} câu hỏi vào ngân hàng (bỏ qua {skippedCount} câu trùng lặp).";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAddQuestionsFromBankAsync(int id, List<int> selectedQuestionBankIds)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (selectedQuestionBankIds == null || !selectedQuestionBankIds.Any())
        {
            TempData["ErrorMessage"] = "Không có câu hỏi nào được chọn từ Ngân hàng câu hỏi.";
            return RedirectToPage(new { id });
        }

        try
        {
            var result = await _lecturerQuizService.AddQuestionsFromBankAsync(id, selectedQuestionBankIds, lecturerId);
            if (result.ImportedCount > 0)
            {
                var msg = $"Lấy thành công {result.ImportedCount} câu hỏi từ Ngân hàng câu hỏi.";
                if (result.SkippedDuplicateCount > 0)
                {
                    msg += $" Đã tự động bỏ qua {result.SkippedDuplicateCount} câu trùng lặp.";
                }
                TempData["SuccessMessage"] = msg;
            }
            else
            {
                if (result.SkippedDuplicateCount > 0)
                {
                    TempData["SuccessMessage"] = $"Không có câu hỏi mới nào được nhập (đã bỏ qua {result.SkippedDuplicateCount} câu trùng lặp đã tồn tại trong đề).";
                }
                else
                {
                    TempData["SuccessMessage"] = "Không có câu hỏi mới nào được nhập.";
                }
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage(new { id });
    }
}
