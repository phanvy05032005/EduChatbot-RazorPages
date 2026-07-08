using EduChatbot.Models;

namespace EduChatbot.Data.Repositories;

public interface IChatRepository
{
    Task<List<ChatConversation>> GetConversationsByUserAsync(string userId);

    Task<List<ChatConversationSummary>> GetConversationSummariesByUserAsync(string userId);

    Task<ChatConversation?> GetConversationWithMessagesAsync(int conversationId, string userId);

    Task<ChatConversation> AddConversationAsync(ChatConversation conversation);

    Task AddMessageAsync(ChatMessage message);

    Task UpdateMessageAsync(ChatMessage message);

    Task UpdateConversationAsync(ChatConversation conversation);

    Task<bool> DeleteConversationAsync(int conversationId, string userId);

    Task<List<ChunkSearchResult>> SearchChunksAsync(float[] queryEmbedding, int? courseId, int topK = 10);

    Task<List<Course>> GetCoursesAsync();
}
