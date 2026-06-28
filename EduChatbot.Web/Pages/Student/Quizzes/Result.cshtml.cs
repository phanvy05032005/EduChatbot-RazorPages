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
public class ResultModel : PageModel
{
    private readonly IStudentQuizService _studentQuizService;

    public ResultModel(IStudentQuizService studentQuizService)
    {
        _studentQuizService = studentQuizService;
    }

    public StudentQuizResultViewModel Result { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int attemptId)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            Result = await _studentQuizService.GetResultAsync(attemptId, studentId);
            return Page();
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Quiz result not found.");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
