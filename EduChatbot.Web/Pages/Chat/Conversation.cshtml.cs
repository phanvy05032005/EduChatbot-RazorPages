using System.Security.Claims;
using System.Text.Json;
using EduChatbot.Business.Services;
using EduChatbot.Web.ViewModels;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Chat;

[Authorize(Roles = ApplicationRoles.Student)]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
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

        var sources = ParseSources(aiMessage.SourceChunks);

        // Detect out-of-scope: no sources and content matches the configured out-of-scope message pattern.
        var isOutOfScope = sources.Count == 0 && !string.IsNullOrWhiteSpace(aiMessage.Content)
            && aiMessage.Content.Contains("ngoài phạm vi", StringComparison.OrdinalIgnoreCase);

        return new JsonResult(new ChatMessageViewModel
        {
            Role = aiMessage.Role,
            Content = aiMessage.Content,
            Sources = sources,
            CreatedAt = aiMessage.CreatedAt.ToString("HH:mm dd/MM/yyyy"),
            IsOutOfScope = isOutOfScope
        });
    }

    /// <summary>
    /// SSE streaming handler: streams AI response token-by-token.
    /// </summary>
    public async Task<IActionResult> OnPostSendMessageStreamAsync(int conversationId, string message)
    {
        var httpResponse = HttpContext.Response;
        httpResponse.ContentType = "text/event-stream";
        httpResponse.Headers.CacheControl = "no-cache";
        httpResponse.Headers.Connection = "keep-alive";

        if (string.IsNullOrWhiteSpace(message))
        {
            await WriteSSEAsync(httpResponse, JsonSerializer.Serialize(new { error = "Please enter a question." }));
            await WriteSSEAsync(httpResponse, JsonSerializer.Serialize(new { done = true }));
            return new EmptyResult();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cancellationToken = HttpContext.RequestAborted;

        await foreach (var data in _chatService.SendMessageStreamAsync(conversationId, userId, message.Trim(), cancellationToken))
        {
            await WriteSSEAsync(httpResponse, data);
        }

        return new EmptyResult();
    }

    private static async Task WriteSSEAsync(HttpResponse response, string data)
    {
        await response.WriteAsync($"data: {data}\n\n");
        await response.Body.FlushAsync();
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
                    Chunk = element.GetProperty("chunk").GetInt32(),
                    Score = element.TryGetProperty("score", out var scoreProp)
                        ? scoreProp.GetDouble() : 0,
                    DocumentId = element.TryGetProperty("documentId", out var docIdProp)
                        ? docIdProp.GetInt32() : 0,
                    ChunkPreview = element.TryGetProperty("chunkPreview", out var previewProp)
                        ? previewProp.GetString() ?? string.Empty : string.Empty
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
