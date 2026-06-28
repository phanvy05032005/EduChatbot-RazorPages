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
}
