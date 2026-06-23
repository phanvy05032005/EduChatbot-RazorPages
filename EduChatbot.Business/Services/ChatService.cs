using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using EduChatbot.Data.Repositories;
using EduChatbot.Models;
using Microsoft.Extensions.Options;

namespace EduChatbot.Business.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _settings;
    private readonly ChatSettings _chatSettings;
    private readonly ISubscriptionAccessService _accessService;

    public ChatService(
        IChatRepository chatRepository,
        IEmbeddingService embeddingService,
        HttpClient httpClient,
        IOptions<OpenRouterSettings> settings,
        IOptions<ChatSettings> chatSettings,
        ISubscriptionAccessService accessService)
    {
        _chatRepository = chatRepository;
        _embeddingService = embeddingService;
        _httpClient = httpClient;
        _settings = settings.Value;
        _chatSettings = chatSettings.Value;
        _accessService = accessService;
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

        string aiResponseText;
        List<ChunkSearchResult> resultsForCitation = [];

        if (IsGreeting(question))
        {
            aiResponseText = "Xin chào! Tôi là trợ lý học tập AI. Tôi có thể giúp gì cho bạn về tài liệu học tập của môn học này?";
        }
        else
        {
            // Step 2: Create embedding for question and search related chunks by cosine similarity with CourseId.
            var questionEmbedding = await _embeddingService.CreateEmbeddingAsync(question);
            var searchResults = await _chatRepository.SearchChunksAsync(questionEmbedding, courseId);

            // Step 3: Check similarity threshold - block out-of-scope questions.
            var relevantResults = searchResults
                .Where(r => r.SimilarityScore >= _chatSettings.SimilarityThreshold)
                .ToList();

            if (relevantResults.Count == 0)
            {
                aiResponseText = _chatSettings.OutOfScopeMessage;
            }
            else
            {
                var context = BuildPromptContext(relevantResults);
                aiResponseText = await CallLlmAsync(question, context);
                resultsForCitation = relevantResults;
            }
        }

        // Step 4: Build enhanced source citation with scores and previews.
        var sourceCitations = BuildSourceCitations(resultsForCitation);

        // Step 5: Save AI response to DB.
        var aiMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Role = "ai",
            Content = aiResponseText,
            SourceChunks = sourceCitations.Count > 0
                ? JsonSerializer.Serialize(sourceCitations)
                : null,
            CreatedAt = DateTime.UtcNow
        };
        await _chatRepository.AddMessageAsync(aiMessage);

        await _accessService.ConsumeChatRequestAsync(userId);

        // Step 6: Update conversation title if this is the first message.
        await UpdateConversationTitleAsync(conversationId, userId, question);

        return aiMessage;
    }

    public async IAsyncEnumerable<string> SendMessageStreamAsync(
        int conversationId, string userId, string question,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
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

        if (IsGreeting(question))
        {
            var greetingText = "Xin chào! Tôi là trợ lý học tập AI. Tôi có thể giúp gì cho bạn về tài liệu học tập của môn học này?";

            // Stream word by word with delay to simulate LLM streaming
            var words = greetingText.Split(' ');
            foreach (var word in words)
            {
                yield return JsonSerializer.Serialize(new { token = word + " " });
                await Task.Delay(40, cancellationToken);
            }

            // Save to DB
            var aiMsg = new ChatMessage
            {
                ConversationId = conversationId,
                Role = "ai",
                Content = greetingText,
                CreatedAt = DateTime.UtcNow
            };
            await _chatRepository.AddMessageAsync(aiMsg);
            await _accessService.ConsumeChatRequestAsync(userId);
            await UpdateConversationTitleAsync(conversationId, userId, question);

            yield return JsonSerializer.Serialize(new { done = true });
            yield break;
        }

        // Step 2: Create embedding and search chunks.
        var questionEmbedding = await _embeddingService.CreateEmbeddingAsync(question);
        var searchResults = await _chatRepository.SearchChunksAsync(questionEmbedding, courseId);

        // Step 3: Check similarity threshold.
        var relevantResults = searchResults
            .Where(r => r.SimilarityScore >= _chatSettings.SimilarityThreshold)
            .ToList();

        if (relevantResults.Count == 0)
        {
            // Out-of-scope: yield immediately, no streaming needed.
            var outOfScopeMsg = _chatSettings.OutOfScopeMessage;
            yield return JsonSerializer.Serialize(new { outOfScope = true, content = outOfScopeMsg });

            // Save to DB.
            var aiMsg = new ChatMessage
            {
                ConversationId = conversationId,
                Role = "ai",
                Content = outOfScopeMsg,
                CreatedAt = DateTime.UtcNow
            };
            await _chatRepository.AddMessageAsync(aiMsg);
            await _accessService.ConsumeChatRequestAsync(userId);
            await UpdateConversationTitleAsync(conversationId, userId, question);

            yield return JsonSerializer.Serialize(new { done = true });
            yield break;
        }

        // Step 4: Build prompt context.
        var context = BuildPromptContext(relevantResults);

        // Step 5: Stream LLM response token-by-token.
        var fullContent = new StringBuilder();
        await foreach (var token in CallLlmStreamAsync(question, context, cancellationToken))
        {
            fullContent.Append(token);
            yield return JsonSerializer.Serialize(new { token });
        }

        // Step 6: Build source citations.
        var sourceCitations = BuildSourceCitations(relevantResults);

        // Step 7: Save complete AI response to DB.
        var aiMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Role = "ai",
            Content = fullContent.ToString(),
            SourceChunks = sourceCitations.Count > 0
                ? JsonSerializer.Serialize(sourceCitations)
                : null,
            CreatedAt = DateTime.UtcNow
        };
        await _chatRepository.AddMessageAsync(aiMessage);
        await _accessService.ConsumeChatRequestAsync(userId);

        // Step 8: Yield sources.
        yield return JsonSerializer.Serialize(new { sources = sourceCitations });

        // Step 9: Update conversation title.
        await UpdateConversationTitleAsync(conversationId, userId, question);

        // Step 10: Yield done.
        yield return JsonSerializer.Serialize(new { done = true });
    }

    private static List<object> BuildSourceCitations(List<ChunkSearchResult> results)
    {
        return results
            .Where(r => r.Chunk.Document != null)
            .Select(r => (object)new
            {
                doc = r.Chunk.Document!.FileName,
                chunk = r.Chunk.ChunkIndex,
                score = Math.Round(r.SimilarityScore, 4),
                documentId = r.Chunk.DocumentId,
                chunkPreview = r.Chunk.Content.Length > 150
                    ? r.Chunk.Content[..150] + "..."
                    : r.Chunk.Content
            })
            .Distinct()
            .ToList();
    }

    private async Task UpdateConversationTitleAsync(int conversationId, string userId, string question)
    {
        var conversation = await _chatRepository.GetConversationWithMessagesAsync(conversationId, userId);
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
    }

    private static string BuildPromptContext(List<ChunkSearchResult> results)
    {
        if (results.Count == 0)
        {
            return "No related documents found in the system.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Below are the relevant document segments found in the system:");
        sb.AppendLine();

        foreach (var result in results)
        {
            var chunk = result.Chunk;
            var docName = chunk.Document?.FileName ?? "Unknown";
            sb.AppendLine($"--- Document: {docName} | Chunk #{chunk.ChunkIndex} | Relevance: {result.SimilarityScore:P0} ---");
            sb.AppendLine(chunk.Content);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private object[] BuildLlmMessages(string question, string context)
    {
        return
        [
            new
            {
                role = "system",
                content = """
                You are the AI learning assistant for the EduChatbot system. Your task is to answer the student's question based ONLY on the content of the documents provided below.
                - Answer in the same language as the student's question.
                - If the documents do not contain the relevant information, clearly state that the question is outside the scope of available documents. Do NOT answer based on general knowledge. Only use information from the provided document context.
                - Always cite source documents when possible, mentioning the document name.
                """
            },
            new
            {
                role = "user",
                content = $"Document context:\n{context}\n\nStudent question: {question}"
            }
        ];
    }

    private async Task<string> CallLlmAsync(string question, string context)
    {
        try
        {
            var requestBody = new
            {
                model = _settings.Model,
                messages = BuildLlmMessages(question, context)
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
                return $"Sorry, the AI system is experiencing issues (HTTP {(int)response.StatusCode}). Please try again later.";
            }

            using var doc = JsonDocument.Parse(responseBody);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content ?? "No response received from AI.";
        }
        catch (Exception ex)
        {
            return $"Sorry, could not connect to AI: {ex.Message}";
        }
    }

    private async IAsyncEnumerable<string> CallLlmStreamAsync(
        string question, string context,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = _settings.Model,
            stream = true,
            messages = BuildLlmMessages(question, context)
        };

        var json = JsonSerializer.Serialize(requestBody);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

        HttpResponseMessage? response = null;
        try
        {
            response = await _httpClient.SendAsync(
                httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                yield return $"Sorry, the AI system is experiencing issues (HTTP {(int)response.StatusCode}). Please try again later.";
                yield break;
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrEmpty(line)) continue;

                // SSE format: "data: {...}" or "data: [DONE]"
                if (!line.StartsWith("data: ")) continue;
                var data = line["data: ".Length..];

                if (data == "[DONE]") break;

                // Parse the SSE JSON chunk from OpenRouter.
                using var chunkDoc = JsonDocument.Parse(data);
                var choices = chunkDoc.RootElement.GetProperty("choices");
                if (choices.GetArrayLength() == 0) continue;

                var delta = choices[0].GetProperty("delta");
                if (delta.TryGetProperty("content", out var contentProp))
                {
                    var tokenText = contentProp.GetString();
                    if (!string.IsNullOrEmpty(tokenText))
                    {
                        yield return tokenText;
                    }
                }
            }
        }
        finally
        {
            response?.Dispose();
        }
    }

    public async Task<List<Course>> GetCoursesAsync()
    {
        return await _chatRepository.GetCoursesAsync();
    }

    private static readonly HashSet<string> Greetings = new(StringComparer.OrdinalIgnoreCase)
    {
        "hello", "hi", "hey", "greetings", "good morning", "good afternoon", "good evening", "yo",
        "xin chào", "xin chao", "chào", "chao", "chào bạn", "chao ban", "hello bạn", "hello ban",
        "hi bạn", "hi ban", "alo", "alô", "helo", "hellooo", "hiii"
    };

    private static bool IsGreeting(string question)
    {
        if (string.IsNullOrWhiteSpace(question)) return false;
        var clean = question.Trim().TrimEnd('?', '!', '.', ',').ToLowerInvariant();
        return Greetings.Contains(clean);
    }

    private static bool ContainsVietnamese(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        string viChars = "àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđ";
        return text.ToLowerInvariant().Any(c => viChars.Contains(c));
    }
}
