using EduChatbot.Models;

namespace EduChatbot.Business.Services;

public interface IChatService
{
    Task<List<ChatConversation>> GetConversationsAsync(string userId);

    Task<List<ChatConversationSummary>> GetConversationSummariesAsync(string userId);

    Task<ChatConversation?> GetConversationAsync(int conversationId, string userId);

    Task<ChatConversation> GetOrCreateConversationAsync(int? conversationId, string userId, int? courseId = null);

    Task<ChatMessage> SendMessageAsync(int conversationId, string userId, string question, string? preferredLanguage = null);

    /// <summary>
    /// Stream AI response token-by-token via SSE.
    /// Yields JSON objects: {"token":"..."} for each token, 
    /// then {"sources":[...]} at the end, then {"done":true}.
    /// If out-of-scope, yields {"outOfScope":true,"content":"..."} immediately.
    /// </summary>
    IAsyncEnumerable<string> SendMessageStreamAsync(int conversationId, string userId, string question, string? preferredLanguage = null, CancellationToken cancellationToken = default);

    Task<bool> DeleteConversationAsync(int conversationId, string userId);

    Task<int> DeleteConversationsAsync(List<int> conversationIds, string userId);

    Task<List<Course>> GetCoursesAsync();
}
