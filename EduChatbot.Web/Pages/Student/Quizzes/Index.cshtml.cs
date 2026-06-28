using System;
using System.Collections.Generic;
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
public class IndexModel : PageModel
{
    private readonly IStudentQuizService _studentQuizService;

    public IndexModel(IStudentQuizService studentQuizService)
    {
        _studentQuizService = studentQuizService;
    }

    public List<Quiz> Quizzes { get; set; } = [];
    public Dictionary<int, int> AttemptCounts { get; set; } = [];
    public Dictionary<int, QuizAttempt> InProgressAttempts { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Quizzes = await _studentQuizService.GetAvailableQuizzesAsync(studentId);

        foreach (var q in Quizzes)
        {
            var submittedCount = await _studentQuizService.GetSubmittedAttemptsCountAsync(q.Id, studentId);
            var inProgress = await _studentQuizService.GetInProgressAttemptAsync(q.Id, studentId);

            AttemptCounts[q.Id] = submittedCount;
            if (inProgress != null)
            {
                InProgressAttempts[q.Id] = inProgress;
            }
        }
        return Page();
    }

    public async Task<IActionResult> OnPostStartAsync(int quizId)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            var attempt = await _studentQuizService.StartQuizAsync(quizId, studentId);
            return RedirectToPage("/Student/Quizzes/Take", new { attemptId = attempt.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage();
        }
    }
}
