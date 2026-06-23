using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Chat;

[Authorize(Roles = ApplicationRoles.Student)]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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

    public async Task<IActionResult> OnPostDeleteConversationAsync([FromForm] int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var success = await _chatService.DeleteConversationAsync(id, userId);
        if (!success)
        {
            return BadRequest(new { error = "Không thể xóa cuộc trò chuyện này hoặc bạn không có quyền." });
        }
        return new JsonResult(new { success = true });
    }
}
