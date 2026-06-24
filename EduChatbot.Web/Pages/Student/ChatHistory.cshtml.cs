using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Student
{
    [Authorize(Roles = ApplicationRoles.Student)]
    public class ChatHistoryModel : PageModel
    {
        private readonly IChatService _chatService;

        public ChatHistoryModel(IChatService chatService)
        {
            _chatService = chatService;
        }

        public List<ChatConversationSummary> Conversations { get; private set; } = [];

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Conversations = await _chatService.GetConversationSummariesAsync(userId);
        }

        public async Task<IActionResult> OnPostDeleteAsync(int chatId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var success = await _chatService.DeleteConversationAsync(chatId, userId);
            if (!success)
            {
                TempData["ErrorMessage"] = "Unable to delete chat.";
                TempData["ErrorKey"] = "student.chathistory.deleteFailed";
            }
            else
            {
                TempData["SuccessMessage"] = "Chat deleted.";
                TempData["SuccessKey"] = "student.chathistory.deleteSuccess";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteMultipleAsync(List<int> chatIds)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (chatIds == null || chatIds.Count == 0)
            {
                TempData["ErrorMessage"] = "No conversations selected for deletion.";
                TempData["ErrorKey"] = "student.chathistory.noChatsSelected";
                return RedirectToPage();
            }

            var deletedCount = await _chatService.DeleteConversationsAsync(chatIds, userId);
            if (deletedCount == 0)
            {
                TempData["ErrorMessage"] = "Unable to delete selected chats.";
                TempData["ErrorKey"] = "student.chathistory.deleteFailed";
            }
            else
            {
                TempData["SuccessMessage"] = $"Successfully deleted the selected chat conversations.";
                TempData["SuccessKey"] = "student.chathistory.deleteMultipleSuccess";
            }
            return RedirectToPage();
        }
    }
}
