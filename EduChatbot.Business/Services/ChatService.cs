using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EduChatbot.Data.Repositories;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduChatbot.Business.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _settings;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISubscriptionAccessService _accessService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IChatRepository chatRepository,
        IEmbeddingService embeddingService,
        HttpClient httpClient,
        UserManager<ApplicationUser> userManager,
        ISubscriptionAccessService accessService,
        IOptions<OpenRouterSettings> settings,
        ILogger<ChatService> logger)
    {
        _chatRepository = chatRepository;
        _embeddingService = embeddingService;
        _httpClient = httpClient;
        _userManager = userManager;
        _accessService = accessService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<List<ChatConversation>> GetConversationsAsync(string userId)
    {
        return await _chatRepository.GetConversationsByUserAsync(userId);
    }

    public async Task<ChatConversation> GetOrCreateConversationAsync(int? conversationId, string userId, int? courseId = null)
    {
        if (conversationId.HasValue)
        {
            var existing = await _chatRepository.GetConversationWithMessagesAsync(conversationId.Value, userId);
            if (existing != null)
            {
                return existing;
            }
        }

        var conversation = new ChatConversation
        {
            UserId = userId,
            Title = "New conversation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CourseId = courseId
        };

        return await _chatRepository.AddConversationAsync(conversation);
    }

    public async Task<ChatMessage> SendMessageAsync(int conversationId, string userId, string question)
    {
        _logger.LogInformation("SendMessageAsync starting for user {UserId}", userId);
        await _accessService.CheckCanChatAsync(userId);

        // Step 1: Save user message to DB.
        var userMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Role = "user",
            Content = question,
            CreatedAt = DateTime.UtcNow
        };
        await _chatRepository.AddMessageAsync(userMessage);

        var conversation = await _chatRepository.GetConversationWithMessagesAsync(conversationId, userId);
        int? courseId = conversation?.CourseId;

        // Step 2: Create embedding for question and search related chunks by cosine similarity with CourseId.
        var questionEmbedding = await _embeddingService.CreateEmbeddingAsync(question);
        var chunks = await _chatRepository.SearchChunksAsync(questionEmbedding, courseId);

        // Step 3: Build prompt context from chunks.
        var context = BuildPromptContext(chunks);

        // Step 4: Call OpenRouter API.
        var aiResponse = await CallLlmAsync(question, context);

        // Step 5: Build source citation.
        var sourceCitations = chunks
            .Where(c => c.Document != null)
            .Select(c => new { doc = c.Document!.FileName, chunk = c.ChunkIndex })
            .Distinct()
            .ToList();

        // Step 6: Save AI response to DB.
        var aiMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Role = "ai",
            Content = aiResponse.Content,
            SourceChunks = sourceCitations.Count > 0
                ? JsonSerializer.Serialize(sourceCitations)
                : null,
            CreatedAt = DateTime.UtcNow
        };
        await _chatRepository.AddMessageAsync(aiMessage);

        _logger.LogInformation("SendMessageAsync: AI response saved. Consuming request quota for user {UserId}", userId);
        await _accessService.ConsumeChatRequestAsync(userId);

        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.UsedTokens += aiResponse.UsedTokens;
            await _userManager.UpdateAsync(user);
        }

        // Step 7: Update conversation title if this is the first message.
        conversation = await _chatRepository.GetConversationWithMessagesAsync(conversationId, userId);
        if (conversation != null)
        {
            var userMessages = conversation.Messages.Where(m => m.Role == "user").ToList();
            if (userMessages.Count == 1)
            {
                conversation.Title = question.Length > 80
                    ? question[..80] + "..."
                    : question;
            }

            conversation.UpdatedAt = DateTime.UtcNow;
            await _chatRepository.UpdateConversationAsync(conversation);
        }

        return aiMessage;
    }

    private static string BuildPromptContext(List<DocumentChunk> chunks)
    {
        if (chunks.Count == 0)
        {
            return "No related documents found in the system.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Below are the relevant document segments found in the system:");
        sb.AppendLine();

        foreach (var chunk in chunks)
        {
            var docName = chunk.Document?.FileName ?? "Unknown";
            sb.AppendLine($"--- Document: {docName} | Chunk #{chunk.ChunkIndex} ---");
            sb.AppendLine(chunk.Content);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private async Task<ChatCompletionResult> CallLlmAsync(string question, string context)
    {
        try
        {
            var requestBody = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = """
                        You are the AI learning assistant for the EduChatbot system. Your task is to answer the student's question based on the content of the documents provided below.
                        - Answer in the same language as the student's question.
                        - If the documents do not contain the relevant information, clearly state that and try to answer based on general knowledge.
                        - Always cite source documents when possible.
                        """
                    },
                    new
                    {
                        role = "user",
                        content = $"Document context:\n{context}\n\nStudent question: {question}"
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new ChatCompletionResult(
                    $"Sorry, the AI system is experiencing issues (HTTP {(int)response.StatusCode}). Please try again later.",
                    EstimateTokenUsage(question, context));
            }

            using var doc = JsonDocument.Parse(responseBody);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            var usedTokens = EstimateTokenUsage(question, context);
            if (doc.RootElement.TryGetProperty("usage", out var usageElement)
                && usageElement.TryGetProperty("total_tokens", out var totalTokensElement)
                && totalTokensElement.TryGetInt32(out var totalTokens)
                && totalTokens > 0)
            {
                usedTokens = totalTokens;
            }

            return new ChatCompletionResult(
                content ?? "No response received from AI.",
                usedTokens);
        }
        catch (Exception ex)
        {
            return new ChatCompletionResult(
                $"Sorry, could not connect to AI: {ex.Message}",
                EstimateTokenUsage(question, context));
        }
    }

    private static int EstimateTokenUsage(string question, string context)
    {
        var totalChars = question.Length + context.Length;
        return Math.Max(1, totalChars / 4);
    }

    public async Task<List<Course>> GetCoursesAsync()
    {
        return await _chatRepository.GetCoursesAsync();
    }

    private sealed record ChatCompletionResult(string Content, int UsedTokens);
}
