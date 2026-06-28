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

[Authorize(Roles = ApplicationRoles.DocumentManagers)]
public class ReviewModel : PageModel
{
    private readonly ILecturerQuizService _lecturerQuizService;
    private readonly IStudentRealtimeNotifier _studentRealtimeNotifier; // SignalR optional

    public ReviewModel(ILecturerQuizService lecturerQuizService, IStudentRealtimeNotifier studentRealtimeNotifier)
    {
        _lecturerQuizService = lecturerQuizService;
        _studentRealtimeNotifier = studentRealtimeNotifier;
    }

    public Quiz Quiz { get; set; } = null!;

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
}
