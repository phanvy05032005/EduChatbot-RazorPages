using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.MVC.Pages.Chat;

[Authorize(Roles = ApplicationRoles.Student)]
public class IndexModel : PageModel
{
    private readonly IChatService _chatService;

    public IndexModel(IChatService chatService)
    {
        _chatService = chatService;
    }

    public List<ChatConversation> Conversations { get; private set; } = [];

    public List<Course> Courses { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Conversations = await _chatService.GetConversationsAsync(userId);
        Courses = await _chatService.GetCoursesAsync();
    }
}
