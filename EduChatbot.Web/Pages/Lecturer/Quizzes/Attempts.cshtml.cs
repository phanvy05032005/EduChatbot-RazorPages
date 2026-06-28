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

namespace EduChatbot.Web.Pages.Lecturer.Quizzes;

[Authorize(Roles = ApplicationRoles.DocumentManagers)]
public class AttemptsModel : PageModel
{
    private readonly ILecturerQuizService _lecturerQuizService;

    public AttemptsModel(ILecturerQuizService lecturerQuizService)
    {
        _lecturerQuizService = lecturerQuizService;
    }

    public Quiz Quiz { get; set; } = null!;
    public List<QuizAttempt> Attempts { get; set; } = [];

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
            Attempts = await _lecturerQuizService.GetQuizAttemptsAsync(id, lecturerId);
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
