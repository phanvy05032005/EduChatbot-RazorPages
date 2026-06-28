using System;
using System.Security.Claims;
using System.Threading.Tasks;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Student.Quizzes;

[Authorize(Roles = ApplicationRoles.Student)]
public class TakeModel : PageModel
{
    private readonly IStudentQuizService _studentQuizService;

    public TakeModel(IStudentQuizService studentQuizService)
    {
        _studentQuizService = studentQuizService;
    }

    [BindProperty]
    public StudentSubmitQuizInput Input { get; set; } = new();

    public StudentTakeQuizViewModel QuizData { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int attemptId)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            QuizData = await _studentQuizService.GetTakeQuizAsync(attemptId, studentId);
            Input.AttemptId = attemptId;
            return Page();
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Attempt not found.");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException)
        {
            // If attempt is already submitted, redirect to result
            return RedirectToPage("/Student/Quizzes/Result", new { attemptId });
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _studentQuizService.SubmitQuizAsync(Input.AttemptId, studentId, Input);
            return RedirectToPage("/Student/Quizzes/Result", new { attemptId = Input.AttemptId, submitted = true });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Failed to submit quiz: " + ex.Message;
            return RedirectToPage("/Student/Quizzes/Index");
        }
    }
}
