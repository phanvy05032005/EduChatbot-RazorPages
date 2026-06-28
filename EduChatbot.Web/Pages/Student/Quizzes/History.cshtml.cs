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
public class HistoryModel : PageModel
{
    private readonly IStudentQuizService _studentQuizService;

    public HistoryModel(IStudentQuizService studentQuizService)
    {
        _studentQuizService = studentQuizService;
    }

    public List<StudentQuizHistoryItemViewModel> Attempts { get; set; } = [];

    public async Task OnGetAsync()
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Attempts = await _studentQuizService.GetHistoryAsync(studentId);
    }
}
