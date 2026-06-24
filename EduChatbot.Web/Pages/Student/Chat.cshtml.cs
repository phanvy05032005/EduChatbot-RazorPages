using System.Security.Claims;
using System.Text.Json;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Student
{
    [Authorize(Roles = ApplicationRoles.Student)]
    public class ChatModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly IDocumentService _documentService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ChatModel(IChatService chatService, IDocumentService documentService, IWebHostEnvironment webHostEnvironment)
        {
            _chatService = chatService;
            _documentService = documentService;
            _webHostEnvironment = webHostEnvironment;
        }

        public ChatConversation ActiveConversation { get; private set; } = new();
        public List<ChatMessageViewModel> ActiveConversationMessages { get; private set; } = [];
        public List<ChatConversationSummary> Conversations { get; private set; } = [];
        public List<Course> Courses { get; private set; } = [];
        public List<Document> ActiveDocuments { get; private set; } = [];

        public int? SelectedCourseId { get; private set; }
        public int? SelectedDocumentId { get; private set; }

        public async Task<IActionResult> OnGetAsync(int? chatId, int? courseId, int? documentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Load student's conversation list
            Conversations = await _chatService.GetConversationSummariesAsync(userId);
            
            // Load courses
            Courses = await _chatService.GetCoursesAsync();

            SelectedCourseId = courseId;
            SelectedDocumentId = documentId;

            if (chatId.HasValue)
            {
                // Retrieve and verify existing conversation directly from DB
                var existing = await _chatService.GetConversationAsync(chatId.Value, userId);
                if (existing == null)
                {
                    TempData["ErrorMessage"] = "This chat conversation does not exist or has been deleted.";
                    return RedirectToPage("/Student/ChatHistory");
                }
                ActiveConversation = existing;
                if (ActiveConversation != null && ActiveConversation.CourseId.HasValue)
                {
                    SelectedCourseId = ActiveConversation.CourseId;
                }
            }
            else
            {
                // Create a temporary conversation or bind it to courseId
                ActiveConversation = await _chatService.GetOrCreateConversationAsync(null, userId, courseId);
            }

            // Map messages to ViewModels and parse SourceChunks JSON
            if (ActiveConversation != null && ActiveConversation.Messages != null)
            {
                ActiveConversationMessages = ActiveConversation.Messages.Select(msg => new ChatMessageViewModel
                {
                    Id = msg.Id,
                    Role = msg.Role ?? "",
                    Content = msg.Content ?? "",
                    SourceChunks = msg.SourceChunks ?? "",
                    Sources = ParseSourceChunks(msg.SourceChunks ?? "")
                }).ToList();
            }

            // Load documents for selected course (only Approved documents)
            if (SelectedCourseId.HasValue)
            {
                var docResult = await _documentService.GetDocumentsAsync(searchTerm: null, currentUserId: null, isAdmin: false, courseId: SelectedCourseId.Value);
                ActiveDocuments = docResult.Documents
                    .Where(d => d.Status == DocumentStatuses.Approved)
                    .ToList();
            }

            return Page();
        }

        private List<ChatSourceViewModel> ParseSourceChunks(string? sourceChunksJson)
        {
            var list = new List<ChatSourceViewModel>();
            if (string.IsNullOrWhiteSpace(sourceChunksJson)) return list;

            try
            {
                using var doc = JsonDocument.Parse(sourceChunksJson);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var src in doc.RootElement.EnumerateArray())
                    {
                        var vm = new ChatSourceViewModel();
                        
                        if (src.TryGetProperty("doc", out var docProp) && docProp.ValueKind == JsonValueKind.String)
                            vm.FileName = docProp.GetString() ?? "";
                            
                        if (src.TryGetProperty("chunk", out var chunkProp))
                        {
                            if (chunkProp.ValueKind == JsonValueKind.Number)
                                vm.ChunkIndex = chunkProp.GetInt32();
                            else if (chunkProp.ValueKind == JsonValueKind.String && int.TryParse(chunkProp.GetString(), out var cIdx))
                                vm.ChunkIndex = cIdx;
                        }
                        
                        if (src.TryGetProperty("score", out var scoreProp))
                        {
                            if (scoreProp.ValueKind == JsonValueKind.Number)
                                vm.Score = scoreProp.GetDouble();
                            else if (scoreProp.ValueKind == JsonValueKind.String && double.TryParse(scoreProp.GetString(), out var sc))
                                vm.Score = sc;
                        }

                        if (vm.Score.HasValue)
                        {
                            vm.ScorePercent = vm.Score.Value <= 1.0
                                ? (int)Math.Round(vm.Score.Value * 100)
                                : (int)Math.Round(vm.Score.Value);
                        }
                        
                        if (src.TryGetProperty("courseId", out var courseIdProp))
                        {
                            if (courseIdProp.ValueKind == JsonValueKind.Number)
                                vm.CourseId = courseIdProp.GetInt32();
                            else if (courseIdProp.ValueKind == JsonValueKind.String && int.TryParse(courseIdProp.GetString(), out var cId))
                                vm.CourseId = cId;
                        }
                        
                        if (src.TryGetProperty("documentId", out var docIdProp))
                        {
                            if (docIdProp.ValueKind == JsonValueKind.Number)
                                vm.DocumentId = docIdProp.GetInt32();
                            else if (docIdProp.ValueKind == JsonValueKind.String && int.TryParse(docIdProp.GetString(), out var dId))
                                vm.DocumentId = dId;
                        }
                        
                        if (src.TryGetProperty("chunkPreview", out var previewProp) && previewProp.ValueKind == JsonValueKind.String)
                        {
                            vm.ChunkPreview = previewProp.GetString() ?? "";
                        }
                        else if (src.TryGetProperty("content", out var contentProp) && contentProp.ValueKind == JsonValueKind.String)
                        {
                            vm.ChunkPreview = contentProp.GetString() ?? "";
                        }

                        list.Add(vm);
                    }
                }
            }
            catch
            {
                // Ignore parse errors, return whatever was parsed so far
            }
            return list;
        }

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

            var preferredLanguage = HttpContext.Request.Cookies["edu_lang"];
            if (preferredLanguage != "vi" && preferredLanguage != "en")
            {
                preferredLanguage = "vi";
            }

            await foreach (var data in _chatService.SendMessageStreamAsync(conversationId, userId, message.Trim(), preferredLanguage, cancellationToken))
            {
                await WriteSSEAsync(httpResponse, data);
            }

            return new EmptyResult();
        }

        public async Task<IActionResult> OnGetGetSourceDetailsAsync(int documentId, int chunkIndex)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Secure retrieval: Student cannot fetch document details with ownership checks using studentId,
            // so we retrieve via GetApprovedDocumentForStudentAsync which verifies the document is approved.
            var doc = await _documentService.GetApprovedDocumentForStudentAsync(documentId);
            if (doc == null)
            {
                return Forbid();
            }

            // Secure verification: Ensure the student has access to the course the document belongs to
            var studentCourses = await _chatService.GetCoursesAsync();
            if (!studentCourses.Any(c => c.Id == doc.CourseId))
            {
                return Forbid();
            }

            var chunk = doc.Chunks.FirstOrDefault(c => c.ChunkIndex == chunkIndex);
            if (chunk == null)
            {
                return new JsonResult(new { success = false, message = "Content not found." });
            }

            return new JsonResult(new
            {
                success = true,
                fileName = doc.FileName,
                documentId = doc.Id,
                chunkIndex = chunkIndex,
                content = chunk.Content
            });
        }

        public async Task<IActionResult> OnGetDownloadDocumentAsync(int documentId)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Secure retrieval: Verify approval status
            var doc = await _documentService.GetApprovedDocumentForStudentAsync(documentId);
            if (doc == null)
            {
                return Forbid();
            }

            // Secure verification: Ensure course is active for the student
            var studentCourses = await _chatService.GetCoursesAsync();
            if (!studentCourses.Any(c => c.Id == doc.CourseId))
            {
                return Forbid();
            }

            var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath, doc.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound("The requested document file does not exist on disk.");
            }

            return PhysicalFile(physicalPath, doc.ContentType, doc.FileName);
        }

        private static async Task WriteSSEAsync(HttpResponse response, string data)
        {
            await response.WriteAsync($"data: {data}\n\n");
            await response.Body.FlushAsync();
        }
    }

    public class ChatMessageViewModel
    {
        public int Id { get; set; }
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
        public string SourceChunks { get; set; } = "";
        public List<ChatSourceViewModel> Sources { get; set; } = new();
    }

    public class ChatSourceViewModel
    {
        public string FileName { get; set; } = "";
        public int? ChunkIndex { get; set; }
        public double? Score { get; set; }
        public int? ScorePercent { get; set; }
        public int CourseId { get; set; }
        public int DocumentId { get; set; }
        public string ChunkPreview { get; set; } = "";
    }
}
