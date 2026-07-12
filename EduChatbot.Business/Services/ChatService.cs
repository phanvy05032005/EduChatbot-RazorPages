using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EduChatbot.Data.Repositories;
using EduChatbot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EduChatbot.Business.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _settings;
    private readonly ChatSettings _chatSettings;
    private readonly ILogger<ChatService> _logger;
    private readonly ISubscriptionAccessService _accessService;

    private static readonly Regex CjkRegex = new(
        @"[\u3040-\u30FF\u3400-\u4DBF\u4E00-\u9FFF\uAC00-\uD7AF]",
        RegexOptions.Compiled);

    public ChatService(
        IChatRepository chatRepository,
        IEmbeddingService embeddingService,
        HttpClient httpClient,
        IOptions<OpenRouterSettings> settings,
        IOptions<ChatSettings> chatSettings,
        ILogger<ChatService> logger,
        ISubscriptionAccessService accessService)
    {
        _chatRepository = chatRepository;
        _embeddingService = embeddingService;
        _httpClient = httpClient;
        _settings = settings.Value;
        _chatSettings = chatSettings.Value;
        _logger = logger;
        _accessService = accessService;
    }

    public async Task<List<ChatConversation>> GetConversationsAsync(string userId)
    {
        return await _chatRepository.GetConversationsByUserAsync(userId);
    }

    public async Task<List<ChatConversationSummary>> GetConversationSummariesAsync(string userId)
    {
        return await _chatRepository.GetConversationSummariesByUserAsync(userId);
    }

    public async Task<ChatConversation?> GetConversationAsync(int conversationId, string userId)
    {
        return await _chatRepository.GetConversationWithMessagesAsync(conversationId, userId);
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

    public async Task<ChatMessage> SendMessageAsync(int conversationId, string userId, string question, string? preferredLanguage = null)
    {
        // Step 1: Validate conversation ownership first.
        var conversation = await _chatRepository.GetConversationWithMessagesAsync(conversationId, userId);
        if (conversation == null)
        {
            throw new UnauthorizedAccessException("Access denied: Conversation does not exist or does not belong to the current user.");
        }

        await _accessService.CheckCanChatAsync(userId);

        // Step 2: Save user message to DB now that ownership and quota are validated.
        var userMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Role = "user",
            Content = question,
            CreatedAt = DateTime.UtcNow
        };
        await _chatRepository.AddMessageAsync(userMessage);
        
        // Add to local message list for history checks
        conversation.Messages.Add(userMessage);

        int? courseId = conversation.CourseId;
        string aiResponseText;
        List<ChunkSearchResult> resultsForCitation = [];

        // Determine target language based on preference or question content
        string targetLang = preferredLanguage ?? (ContainsVietnamese(question) ? "vi" : "vi");

        // Step 3: Course scope validation — block if no course selected
        if (!courseId.HasValue)
        {
            _logger.LogInformation("No course selected (CourseId is null). Blocking RAG retrieval.");
            aiResponseText = targetLang == "en"
                ? "Please select a course first before asking questions. You can choose a course from the sidebar on the left."
                : "Vui lòng chọn một môn học trước khi đặt câu hỏi. Bạn có thể chọn môn học từ thanh bên trái.";
        }
        // Step 4: Detect strict greeting
        else if (IsStrictGreeting(question))
        {
            aiResponseText = targetLang == "en"
                ? "Hello! I am your AI learning assistant. How can I help you with the learning materials of this course?"
                : "Xin chào! Tôi là trợ lý học tập AI. Tôi có thể giúp gì cho bạn về tài liệu học tập của môn học này?";
        }
        // Step 5: Detect unclear intent
        else if (IsUnclearIntent(question, targetLang, conversation.Messages, out var clarificationMessage))
        {
            aiResponseText = clarificationMessage;
        }
        else
        {
            // Step 6: Normal RAG flow — courseId is guaranteed non-null here
            var questionEmbedding = await _embeddingService.CreateEmbeddingAsync(question);
            var searchResults = await _chatRepository.SearchChunksAsync(questionEmbedding, courseId);

            var relevantResults = searchResults
                .Where(r => r.SimilarityScore >= _chatSettings.SimilarityThreshold)
                .ToList();

            if (relevantResults.Count == 0)
            {
                aiResponseText = targetLang == "en"
                    ? "I could not find enough relevant information in the course documents to answer this question confidently. Please try asking a question related to the uploaded course materials."
                    : _chatSettings.OutOfScopeMessage;
            }
            else
            {
                var deduped = DeduplicateChunks(relevantResults);
                var ctxChunks = deduped.Take(8).ToList();
                var context = BuildPromptContext(ctxChunks);
                aiResponseText = await CallLlmAsync(question, context, targetLang);
                resultsForCitation = SelectDisplaySources(ctxChunks, maxDisplay: 3);
            }
        }

        // Step 6: Build citations
        var sourceCitations = BuildSourceCitations(resultsForCitation);

        // Step 7: Save AI response to DB
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

        // Step 8: Update title and return.
        await UpdateConversationTitleAsync(conversationId, userId, question);

        return aiMessage;
    }

    public async IAsyncEnumerable<string> SendMessageStreamAsync(
        int conversationId, string userId, string question, string? preferredLanguage = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Step 1: Validate conversation ownership first.
        var conversation = await _chatRepository.GetConversationWithMessagesAsync(conversationId, userId);
        if (conversation == null)
        {
            yield return JsonSerializer.Serialize(new { error = "Access denied: Conversation does not exist or does not belong to the current user." });
            yield break;
        }

        await _accessService.CheckCanChatAsync(userId);

        // Step 2: Save user message to DB now that ownership and quota are validated.
        var userMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Role = "user",
            Content = question,
            CreatedAt = DateTime.UtcNow
        };
        await _chatRepository.AddMessageAsync(userMessage);
        
        // Add to local message list for history checks
        conversation.Messages.Add(userMessage);

        int? courseId = conversation.CourseId;
        var targetLanguage = preferredLanguage ?? "vi";

        // Step 3: Course scope validation — block if no course selected
        if (!courseId.HasValue)
        {
            _logger.LogInformation("No course selected (CourseId is null). Blocking RAG retrieval for streaming.");
            var noCourseMsg = targetLanguage == "en"
                ? "Please select a course first before asking questions. You can choose a course from the sidebar on the left."
                : "Vui lòng chọn một môn học trước khi đặt câu hỏi. Bạn có thể chọn môn học từ thanh bên trái.";

            var words = noCourseMsg.Split(' ');
            foreach (var word in words)
            {
                yield return JsonSerializer.Serialize(new { token = word + " " });
                await Task.Delay(40, cancellationToken);
            }

            var aiMsg = new ChatMessage
            {
                ConversationId = conversationId,
                Role = "ai",
                Content = noCourseMsg,
                CreatedAt = DateTime.UtcNow
            };
            await _chatRepository.AddMessageAsync(aiMsg);
            await UpdateConversationTitleAsync(conversationId, userId, question);

            yield return JsonSerializer.Serialize(new { done = true });
            yield break;
        }

        // Step 4: Detect strict greeting
        if (IsStrictGreeting(question))
        {
            var greetingText = targetLanguage == "en"
                ? "Hello! I am your AI learning assistant. How can I help you with the learning materials of this course?"
                : "Xin chào! Tôi là trợ lý học tập AI. Tôi có thể giúp gì cho bạn về tài liệu học tập của môn học này?";

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
            var updatedSub = await _accessService.GetCurrentSubscriptionAsync(userId);
            if (updatedSub != null)
            {
                yield return JsonSerializer.Serialize(new { type = "quota", remainingRequests = updatedSub.RemainingRequests, requestLimit = updatedSub.Plan.RequestLimit });
            }
            await UpdateConversationTitleAsync(conversationId, userId, question);

            yield return JsonSerializer.Serialize(new { done = true });
            yield break;
        }

        // Step 5: Detect unclear intent
        if (IsUnclearIntent(question, targetLanguage, conversation.Messages, out var clarificationMessage))
        {
            // Stream the clarification message word by word
            var words = clarificationMessage.Split(' ');
            foreach (var word in words)
            {
                yield return JsonSerializer.Serialize(new { token = word + " " });
                await Task.Delay(40, cancellationToken);
            }

            // Save clarification response to DB
            var aiMsg = new ChatMessage
            {
                ConversationId = conversationId,
                Role = "ai",
                Content = clarificationMessage,
                CreatedAt = DateTime.UtcNow
            };
            await _chatRepository.AddMessageAsync(aiMsg);
            await UpdateConversationTitleAsync(conversationId, userId, question);

            yield return JsonSerializer.Serialize(new { done = true });
            yield break;
        }

        // Step 5: Create embedding and search chunks.
        var questionEmbedding = await _embeddingService.CreateEmbeddingAsync(question);
        var searchResults = await _chatRepository.SearchChunksAsync(questionEmbedding, courseId);

        // Step 6: Check similarity threshold.
        var relevantResults = searchResults
            .Where(r => r.SimilarityScore >= _chatSettings.SimilarityThreshold)
            .ToList();

        // Step 6b: Deduplicate and diversify.
        var dedupedResults = DeduplicateChunks(relevantResults);
        var contextChunks = dedupedResults.Take(8).ToList();
        var displaySources = SelectDisplaySources(contextChunks, maxDisplay: 3);

        // Safe developer logging of the flow metrics
        _logger.LogInformation("--- Chat Request Info ---");
        _logger.LogInformation("User Question: {Question}", question);
        _logger.LogInformation("Model Name: {Model}", _settings.Model);
        _logger.LogInformation("Target Language: {Language}", targetLanguage);

        // Scan retrieved chunks for CJK and print metadata logs
        _logger.LogInformation("Top retrieved chunks (Count: {Count}):", dedupedResults.Count);
        foreach (var r in dedupedResults)
        {
            var chunk = r.Chunk;
            bool hasCjk = CjkRegex.IsMatch(chunk.Content);
            if (hasCjk)
            {
                _logger.LogWarning("CJK characters detected in retrieved chunk! DocumentId: {DocId}, ChunkIndex: {ChunkIndex}, File: {FileName}", 
                    chunk.DocumentId, chunk.ChunkIndex, chunk.Document?.FileName ?? "Unknown");
            }

            var snippet = chunk.Content.Length > 500 ? chunk.Content[..500] : chunk.Content;
            _logger.LogInformation("  - File: {FileName} | ChunkIndex: {ChunkIndex} | Score: {Score:F4} | Contains CJK: {ContainsCjk}\n    Raw Text (500 chars): {Snippet}",
                chunk.Document?.FileName ?? "Unknown", chunk.ChunkIndex, r.SimilarityScore, hasCjk, snippet);
        }

        if (dedupedResults.Count == 0)
        {
            // Out-of-scope: yield immediately, no streaming needed.
            var outOfScopeMsg = targetLanguage == "en"
                ? "I could not find enough relevant information in the course documents to answer this question confidently. Please try asking a question related to the uploaded course materials."
                : _chatSettings.OutOfScopeMessage;
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
            var updatedSub = await _accessService.GetCurrentSubscriptionAsync(userId);
            if (updatedSub != null)
            {
                yield return JsonSerializer.Serialize(new { type = "quota", remainingRequests = updatedSub.RemainingRequests, requestLimit = updatedSub.Plan.RequestLimit });
            }
            await UpdateConversationTitleAsync(conversationId, userId, question);

            yield return JsonSerializer.Serialize(new { done = true });
            yield break;
        }

        // Step 7: Build prompt context from deduplicated, diverse chunks.
        var context = BuildPromptContext(contextChunks);

        // Full prompt summary/context (only in Development environment)
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            var messagesSummary = BuildLlmMessages(question, context, targetLanguage);
            _logger.LogInformation("--- Final Prompt Context (Development Mode Only) ---");
            _logger.LogInformation("System Prompt: {SystemPrompt}", JsonSerializer.Serialize(messagesSummary[0]));
            _logger.LogInformation("Context and Question:\n{ContextAndQuestion}", context);
        }

        // Step 8: Save placeholder AI message BEFORE streaming (prevents orphan on cancel).
        var aiMessage = new ChatMessage
        {
            ConversationId = conversationId,
            Role = "ai",
            Content = "AI is thinking...",
            CreatedAt = DateTime.UtcNow
        };
        await _chatRepository.AddMessageAsync(aiMessage);

        // Step 9: Stream LLM response token-by-token.
        var fullContent = new StringBuilder();
        await foreach (var token in CallLlmStreamAsync(question, context, targetLanguage, cancellationToken))
        {
            fullContent.Append(token);
            yield return JsonSerializer.Serialize(new { token });
        }

        // Safe check for CJK characters in the output
        var finalResponse = fullContent.ToString();
        if (CjkRegex.IsMatch(finalResponse))
        {
            _logger.LogWarning("CJK characters detected in final model response output! Response: {ResponseText}", finalResponse);
        }
        else
        {
            _logger.LogInformation("Final Model Response (No CJK detected): {ResponseText}", finalResponse);
        }

        // Step 10: Build source citations from display sources.
        var sourceCitations = BuildSourceCitations(displaySources);

        // Step 11: Update placeholder with real response + sources.
        aiMessage.Content = finalResponse;
        aiMessage.SourceChunks = sourceCitations.Count > 0
            ? JsonSerializer.Serialize(sourceCitations)
            : null;
        await _chatRepository.UpdateMessageAsync(aiMessage);

        // Step 12: Re-check quota before consumption (TOCTOU fix).
        await _accessService.CheckCanChatAsync(userId);
        await _accessService.ConsumeChatRequestAsync(userId);

        // Step 13: Yield sources.
        yield return JsonSerializer.Serialize(new { sources = sourceCitations });

        // Yield updated quota event
        var finalSub = await _accessService.GetCurrentSubscriptionAsync(userId);
        if (finalSub != null)
        {
            yield return JsonSerializer.Serialize(new { type = "quota", remainingRequests = finalSub.RemainingRequests, requestLimit = finalSub.Plan.RequestLimit });
        }

        // Step 14: Update conversation title.
        await UpdateConversationTitleAsync(conversationId, userId, question);

        // Step 15: Yield done.
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

    /// <summary>
    /// Removes CJK (Chinese/Japanese/Korean) characters from a text string.
    /// These characters may appear in document chunks due to OCR noise or mixed-language PDFs.
    /// Stripping them before passing to the LLM prevents the model from copying them into the output.
    /// </summary>
    private static string SanitizeCjkFromContext(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        // Replace CJK characters with a space so surrounding words stay readable
        return CjkRegex.Replace(text, " ");
    }

    private static string BuildPromptContext(List<ChunkSearchResult> results)
    {
        if (results.Count == 0)
        {
            return "No related documents found in the system.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Below are the relevant document segments found in the system. You may use information from one or multiple sources to answer comprehensively:");
        sb.AppendLine();

        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            var chunk = result.Chunk;
            var docName = chunk.Document?.FileName ?? "Unknown";
            var sanitizedContent = SanitizeCjkFromContext(chunk.Content);
            sb.AppendLine($"[Source {i + 1}] Document: {docName} | Chunk #{chunk.ChunkIndex} | Relevance: {result.SimilarityScore:P0}");
            sb.AppendLine(sanitizedContent);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private object[] BuildLlmMessages(string question, string context, string targetLanguage)
    {
        string langName = targetLanguage == "vi" ? "Vietnamese" : "English";
        string langExamples = targetLanguage == "vi"
            ? "Write \"nhóm\" not \"チーム\". Write \"triển khai\" not \"展開\". Write \"đội ngũ\" not \"團隊\"."
            : "Write \"team\" not \"チーム\". Write \"deploy\" not \"展開\".";  

        return
        [
            new
            {
                role = "system",
                content = $"""
                You are the AI learning assistant for the EduChatbot system.
                
                === ABSOLUTE LANGUAGE RULE (HIGHEST PRIORITY) ===
                You MUST write your ENTIRE response ONLY in {langName}.
                
                STRICTLY FORBIDDEN — you will NEVER output these character types:
                1. Chinese characters (汉字/漢字): U+4E00–U+9FFF, U+3400–U+4DBF
                2. Japanese Hiragana (ひらがな): U+3040–U+309F
                3. Japanese Katakana (カタカナ): U+30A0–U+30FF
                4. Korean Hangul (한글): U+AC00–U+D7AF
                
                If the document context contains any of the forbidden characters above, you MUST:
                - Translate the meaning into {langName}, OR
                - Drop the character entirely if it has no meaning.
                - NEVER copy or reproduce the forbidden characters.
                
                Correct examples: {langExamples}
                
                === TASK ===
                Answer the student's question based ONLY on the document context provided below.
                - The context may come from MULTIPLE documents. If the question requires information from several sources, synthesize them into a coherent answer.
                - Provide DETAILED, IN-DEPTH explanations. Go beyond surface-level definitions — explain the WHY, not just the WHAT.
                - Structure your answer with clear sections when the topic is complex (e.g., overview, key concepts, practical steps, examples).
                - Include concrete EXAMPLES or code snippets from the context when relevant.
                - When appropriate, mention which documents or sources you are drawing from (e.g., "According to Lecture 03..." or "As covered in the EF Core document...").
                - Do NOT use general knowledge outside the documents.
                - If the documents do not contain sufficient information to answer, say so clearly rather than guessing.
                - You may keep standard English technical terms (e.g. API, microservice, database, CI/CD, framework) even when the target language is Vietnamese.
                - If the user's message is not a clear academic question or learning request, ask the user to clarify instead of answering directly. (Nếu tin nhắn của người dùng không phải là câu hỏi học tập hoặc yêu cầu học tập rõ ràng, hãy yêu cầu người dùng hỏi rõ hơn. Không trả lời trực tiếp chỉ vì tin nhắn có chứa từ khóa xuất hiện trong tài liệu.)
                - Do NOT fabricate information. If the answer cannot be found in the provided context, state that clearly.
                """
            },
            new
            {
                role = "user",
                content = $"Document context:\n{context}\n\nStudent question: {question}"
            }
        ];
    }

    private async Task<string> CallLlmAsync(string question, string context, string targetLanguage)
    {
        try
        {
            var requestBody = new
            {
                model = _settings.Model,
                temperature = _settings.Temperature,
                messages = BuildLlmMessages(question, context, targetLanguage)
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
            _logger.LogError(ex, "LLM API call failed");
            return $"Sorry, could not connect to AI. Please try again later.";
        }
    }

    private async IAsyncEnumerable<string> CallLlmStreamAsync(
        string question, string context, string targetLanguage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = _settings.Model,
            stream = true,
            temperature = _settings.Temperature,
            messages = BuildLlmMessages(question, context, targetLanguage)
        };

        var json = JsonSerializer.Serialize(requestBody);

        const int maxRetries = 3;
        HttpResponseMessage? response = null;
        string? errorMessage = null;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.BaseUrl)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);

                response = await _httpClient.SendAsync(
                    httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if ((int)response.StatusCode == 429 || (int)response.StatusCode >= 500)
                {
                    if (attempt < maxRetries - 1)
                    {
                        var delay = (int)Math.Pow(2, attempt + 1) * 1000;
                        _logger.LogWarning("LLM API returned {StatusCode}, retrying in {Delay}s (attempt {Attempt}/{MaxRetries})",
                            (int)response.StatusCode, delay / 1000, attempt + 1, maxRetries);
                        response.Dispose();
                        response = null;
                        await Task.Delay(delay, cancellationToken);
                        continue;
                    }
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("LLM API returned HTTP {StatusCode}", (int)response.StatusCode);
                    errorMessage = $"Sorry, the AI system is experiencing issues (HTTP {(int)response.StatusCode}). Please try again later.";
                    break;
                }

                break;
            }
            catch (HttpRequestException ex) when (attempt < maxRetries - 1)
            {
                _logger.LogWarning(ex, "LLM API request failed, retrying (attempt {Attempt}/{MaxRetries})", attempt + 1, maxRetries);
                await Task.Delay(1000 * (attempt + 1), cancellationToken);
            }
        }

        if (errorMessage != null)
        {
            yield return errorMessage;
            yield break;
        }

        if (response == null || !response.IsSuccessStatusCode)
        {
            yield break;
        }

        try
        {
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrEmpty(line)) continue;

                if (!line.StartsWith("data: ")) continue;
                var data = line["data: ".Length..];

                if (data == "[DONE]") break;

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

    public async Task<bool> DeleteConversationAsync(int conversationId, string userId)
    {
        return await _chatRepository.DeleteConversationAsync(conversationId, userId);
    }

    public async Task<int> DeleteConversationsAsync(List<int> conversationIds, string userId)
    {
        if (conversationIds == null || conversationIds.Count == 0)
        {
            return 0;
        }

        int deletedCount = 0;
        foreach (var id in conversationIds)
        {
            if (await _chatRepository.DeleteConversationAsync(id, userId))
            {
                deletedCount++;
            }
        }
        return deletedCount;
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

    private static readonly HashSet<string> StrictlyGreetings = new(StringComparer.OrdinalIgnoreCase)
    {
        "hi", "hello", "chào", "xin chào", "chào bạn", "chao ban", "hey"
    };

    private static bool IsStrictGreeting(string question)
    {
        if (string.IsNullOrWhiteSpace(question)) return false;
        var clean = question.Trim().TrimEnd('?', '!', '.', ',').ToLowerInvariant();
        return StrictlyGreetings.Contains(clean);
    }

    private bool IsUnclearIntent(
        string question,
        string targetLanguage,
        IReadOnlyList<ChatMessage> recentHistory,
        out string clarificationMessage)
    {
        clarificationMessage = "";
        if (string.IsNullOrWhiteSpace(question)) return false;

        var cleanQuestion = question.Trim().ToLowerInvariant();
        bool hasOriginalQuestionMark = question.Contains("?");

        // Normalize punctuation
        cleanQuestion = Regex.Replace(cleanQuestion, @"[?!.,\s]+$", "").Trim();

        // 1. Identify common tech terms in the user prompt for topic extraction
        var techKeywords = new[] { "microservice", "monolithic", "database", "api", "git", "ci/cd", "cicd", "docker", "kubernetes" };
        string? extractedTopic = null;
        foreach (var kw in techKeywords)
        {
            if (cleanQuestion.Contains(kw))
            {
                extractedTopic = kw switch
                {
                    "microservice" => "Microservice",
                    "monolithic" => "Monolithic",
                    "database" => "Database",
                    "api" => "API",
                    "git" => "Git",
                    "ci/cd" or "cicd" => "CI/CD",
                    "docker" => "Docker",
                    "kubernetes" => "Kubernetes",
                    _ => kw
                };
                break;
            }
        }

        if (extractedTopic == null)
        {
            var match = Regex.Match(cleanQuestion, @"\b(?:là|la|is|im|am)\s+([a-zA-Z0-9_-]+)");
            if (match.Success)
            {
                var word = match.Groups[1].Value;
                if (word.Length > 2)
                {
                    extractedTopic = char.ToUpper(word[0]) + word[1..];
                }
            }
        }

        // 2. Define learning markers that indicate a valid question/request
        var learningMarkers = new[] {
            "là gì", "la gi", "giải thích", "giai thich", "tóm tắt", "tom tat", 
            "so sánh", "so sanh", "phân tích", "phan tich", "hướng dẫn", "huong dan", 
            "chỉ", "chi", "liệt kê", "liet ke", "cho ví dụ", "cho vi du", "nêu", "neu", 
            "sao", "thế nào", "the nao", "nào", "nao", "hỏi", "hoi", "khác nhau", "khac nhau", 
            "how", "what", "why", "explain", "summarize", "compare", "describe", 
            "list", "define", "give example", "show", "definition", "concept", "học", "hoc"
        };

        bool hasLearningMarker = learningMarkers.Any(m => cleanQuestion.Contains(m));

        // 3. Always protect valid academic questions (if learning marker is present, it's NOT unclear)
        if (hasLearningMarker)
        {
            return false;
        }

        // 4. Detect identity declarations: starts with or contains identity prefixes + has a tech keyword/extracted topic
        var identityPrefixes = new[] {
            "tôi là", "toi la", "tui là", "tui la", "tớ là", "to la", 
            "mình là", "minh la", "i am", "i'm", "im"
        };
        bool isIdentityDeclaration = identityPrefixes.Any(p => cleanQuestion.StartsWith(p) || cleanQuestion.Contains(" " + p + " "));

        // 5. Detect joke/slang endings or spam patterns
        var unclearSlangs = new[] { "haha", "hihi", "hehe", "lol", "đẹp trai", "dep trai", "xịn", "xin" };
        bool endsWithSlang = unclearSlangs.Any(s => cleanQuestion.EndsWith(s) || cleanQuestion.Contains(" " + s));

        // 6. Block identity statements or joke slangs immediately if they contain tech topics
        bool isUnclear = false;
        if (isIdentityDeclaration && extractedTopic != null)
        {
            isUnclear = true;
        }
        else if (endsWithSlang && extractedTopic != null)
        {
            isUnclear = true;
        }

        if (isUnclear)
        {
            if (targetLanguage == "en")
            {
                string topicText = extractedTopic ?? "this topic";
                clarificationMessage = $"I'm not sure what you want to ask about {topicText}. Please ask a clearer question, such as: “What is {topicText}?”, “What are the advantages of {topicText}?” or “How is {topicText} different from Monolithic?”.";
            }
            else
            {
                string topicText = extractedTopic ?? "chủ đề này";
                clarificationMessage = $"Mình chưa rõ bạn muốn hỏi gì về {topicText}. Bạn có thể hỏi cụ thể hơn, ví dụ: “{topicText} là gì?”, “Ưu điểm của {topicText} là gì?” hoặc “{topicText} khác Monolithic như thế nào?”.";
            }
            return true;
        }

        // 7. Define follow-up rules (don't block actual follow-up queries if the conversation has history)
        var hasPreviousTurn = recentHistory != null && recentHistory.Any(m => m.Role == "ai");
        if (hasPreviousTurn)
        {
            var followUpKeywords = new[] { 
                "nó là gì", "no la gi", "nó có", "no co", "còn", "con", "ví dụ", "vi du", 
                "lợi ích", "loi ich", "tại sao", "tai sao", "nhược điểm", "nhuoc diem", 
                "ưu điểm", "uu diem", "how about", "what about", "why", "example", 
                "advantage", "disadvantage", "benefit", "khác", "khac", "so sánh", "so sanh"
            };
            // If query is short or has a follow-up keyword, do not block it
            if (cleanQuestion.Split(' ').Length <= 4 || followUpKeywords.Any(k => cleanQuestion.Contains(k)))
            {
                return false; 
            }
        }

        // 8. Block if it's just a single technical word (like "Microservice") without a question mark or learning marker (if no history)
        if (extractedTopic != null && cleanQuestion.Split(' ').Length <= 2 && !hasOriginalQuestionMark)
        {
            if (targetLanguage == "en")
            {
                string topicText = extractedTopic ?? "this topic";
                clarificationMessage = $"I'm not sure what you want to ask about {topicText}. Please ask a clearer question, such as: “What is {topicText}?”, “What are the advantages of {topicText}?” or “How is {topicText} different from Monolithic?”.";
            }
            else
            {
                string topicText = extractedTopic ?? "chủ đề này";
                clarificationMessage = $"Mình chưa rõ bạn muốn hỏi gì về {topicText}. Bạn có thể hỏi cụ thể hơn, ví dụ: “{topicText} là gì?”, “Ưu điểm của {topicText} là gì?” hoặc “{topicText} khác Monolithic như thế nào?”.";
            }
            return true;
        }

        return false;
    }

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

    /// <summary>
    /// Removes near-duplicate chunks based on content word overlap.
    /// When two chunks share >70% of their unique words, keep the one with higher score.
    /// </summary>
    private static List<ChunkSearchResult> DeduplicateChunks(List<ChunkSearchResult> results)
    {
        if (results.Count <= 1) return results;

        var deduped = new List<ChunkSearchResult>();
        var usedWordSets = new List<HashSet<string>>();

        foreach (var result in results.OrderByDescending(r => r.SimilarityScore))
        {
            var words = result.Chunk.Content
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim().ToLowerInvariant())
                .Where(w => w.Length > 2)
                .ToHashSet();

            if (words.Count == 0)
            {
                deduped.Add(result);
                continue;
            }

            bool isDuplicate = false;
            foreach (var existing in usedWordSets)
            {
                var intersection = words.Intersect(existing).Count();
                var union = words.Union(existing).Count();
                var jaccard = (double)intersection / union;

                if (jaccard > 0.7)
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                deduped.Add(result);
                usedWordSets.Add(words);
            }
        }

        return deduped;
    }

    /// <summary>
    /// Selects display sources with document diversity.
    /// Ensures at least 1 chunk from each unique document, limited to maxDisplay total.
    /// </summary>
    private static List<ChunkSearchResult> SelectDisplaySources(List<ChunkSearchResult> contextChunks, int maxDisplay)
    {
        if (contextChunks.Count == 0) return contextChunks;

        var display = new List<ChunkSearchResult>();
        var seenDocs = new HashSet<int>();

        // Pass 1: pick best chunk from each unique document
        foreach (var result in contextChunks.OrderByDescending(r => r.SimilarityScore))
        {
            var docId = result.Chunk.DocumentId;
            if (!seenDocs.Contains(docId))
            {
                display.Add(result);
                seenDocs.Add(docId);
                if (display.Count >= maxDisplay) break;
            }
        }

        // Pass 2: if we still have room, fill with best remaining chunks from already-seen docs
        if (display.Count < maxDisplay)
        {
            foreach (var result in contextChunks
                .Where(r => !display.Contains(r))
                .OrderByDescending(r => r.SimilarityScore))
            {
                display.Add(result);
                if (display.Count >= maxDisplay) break;
            }
        }

        return display;
    }
}
