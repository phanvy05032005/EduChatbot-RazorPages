using System.Security.Claims;
using System.Text.Json;
using EduChatbot.Business.Services;
using EduChatbot.MVC.Models;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.MVC.Pages.Chat;

[Authorize(Roles = ApplicationRoles.Student)]
public class ConversationModel : PageModel
{
    private readonly IChatService _chatService;

    public ConversationModel(IChatService chatService)
    {
        _chatService = chatService;
    }

    public ChatConversation Conversation { get; private set; } = new();

    public IReadOnlyList<ChatConversation> Conversations { get; private set; } = [];

    public async Task OnGetAsync(int? id, int? courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Conversation = await _chatService.GetOrCreateConversationAsync(id, userId, courseId);
        Conversations = await _chatService.GetConversationsAsync(userId);
    }

    public async Task<IActionResult> OnPostSendMessageAsync(int conversationId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return BadRequest(new { error = "Please enter a question." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var aiMessage = await _chatService.SendMessageAsync(conversationId, userId, message.Trim());

        return new JsonResult(new ChatMessageViewModel
        {
            Role = aiMessage.Role,
            Content = aiMessage.Content,
            Sources = ParseSources(aiMessage.SourceChunks),
            CreatedAt = aiMessage.CreatedAt.ToString("HH:mm dd/MM/yyyy")
        });
    }

    private static List<ChatSourceViewModel> ParseSources(string? sourceChunks)
    {
        var sources = new List<ChatSourceViewModel>();
        if (string.IsNullOrWhiteSpace(sourceChunks))
        {
            return sources;
        }

        try
        {
            using var doc = JsonDocument.Parse(sourceChunks);
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                sources.Add(new ChatSourceViewModel
                {
                    Doc = element.GetProperty("doc").GetString() ?? string.Empty,
                    Chunk = element.GetProperty("chunk").GetInt32()
                });
            }
        }
        catch
        {
            // Ignore malformed source metadata so the answer can still render.
        }

        return sources;
    }
}
