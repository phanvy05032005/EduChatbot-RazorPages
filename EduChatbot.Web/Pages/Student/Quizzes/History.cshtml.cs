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
    private readonly ISubscriptionAccessService _subscriptionAccessService;

    public HistoryModel(IStudentQuizService studentQuizService, ISubscriptionAccessService subscriptionAccessService)
    {
        _studentQuizService = studentQuizService;
        _subscriptionAccessService = subscriptionAccessService;
    }

    public List<StudentQuizHistoryItemViewModel> Attempts { get; set; } = [];
    public bool CanUseQuiz { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        CanUseQuiz = await _subscriptionAccessService.CheckCanUseQuizAsync(studentId);
        Attempts = await _studentQuizService.GetHistoryAsync(studentId);
        return Page();
    }
}
