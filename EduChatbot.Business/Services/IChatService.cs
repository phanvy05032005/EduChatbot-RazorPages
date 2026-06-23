using EduChatbot.Models;

namespace EduChatbot.Business.Services;

public interface IChatService
{
    Task<List<ChatConversation>> GetConversationsAsync(string userId);

    Task<ChatConversation> GetOrCreateConversationAsync(int? conversationId, string userId, int? courseId = null);

    Task<ChatMessage> SendMessageAsync(int conversationId, string userId, string question);

    /// <summary>
    /// Stream AI response token-by-token via SSE.
    /// Yields JSON objects: {"token":"..."} for each token, 
    /// then {"sources":[...]} at the end, then {"done":true}.
    /// If out-of-scope, yields {"outOfScope":true,"content":"..."} immediately.
    /// </summary>
    IAsyncEnumerable<string> SendMessageStreamAsync(int conversationId, string userId, string question, CancellationToken cancellationToken = default);

    Task<bool> DeleteConversationAsync(int conversationId, string userId);

    Task<List<Course>> GetCoursesAsync();
}
