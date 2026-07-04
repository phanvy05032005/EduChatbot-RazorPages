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
public class IndexModel : PageModel
{
    private readonly ILecturerQuizService _lecturerQuizService;

    public IndexModel(ILecturerQuizService lecturerQuizService)
    {
        _lecturerQuizService = lecturerQuizService;
    }

    public List<Quiz> Quizzes { get; set; } = [];

    public async Task OnGetAsync()
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Quizzes = await _lecturerQuizService.GetLecturerQuizzesAsync(lecturerId);
    }

    public async Task<IActionResult> OnPostArchiveAsync(int id)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        try
        {
            await _lecturerQuizService.ArchiveQuizAsync(id, lecturerId);
            TempData["SuccessMessage"] = "Quiz archived successfully. Students will no longer see this quiz for new attempts, but existing attempts and results will be kept.";
            return RedirectToPage();
        }
        catch (System.Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnGetDeleteImpactAsync(int id)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(lecturerId))
        {
            return Challenge();
        }

        try
        {
            var isAdmin = User.IsInRole(ApplicationRoles.Admin);
            var impact = await _lecturerQuizService.GetDeleteImpactAsync(id, lecturerId, isAdmin);
            return new JsonResult(impact);
        }
        catch (System.Exception ex)
        {
            return new JsonResult(new { error = ex.Message }) { StatusCode = 400 };
        }
    }

    public async Task<IActionResult> OnPostDeleteConfirmAsync(int id, string action)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(lecturerId))
        {
            return Challenge();
        }

        try
        {
            var isAdmin = User.IsInRole(ApplicationRoles.Admin);
            var result = await _lecturerQuizService.ExecuteDeleteOrArchiveAsync(id, action, lecturerId, isAdmin);
            return new JsonResult(result);
        }
        catch (System.Exception ex)
        {
            return new JsonResult(new { error = ex.Message }) { StatusCode = 400 };
        }
    }
}
