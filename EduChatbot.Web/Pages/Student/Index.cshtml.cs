using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Student
{
    [Authorize(Roles = ApplicationRoles.Student)]
    public class IndexModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly IDocumentService _documentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IChatService chatService, 
            IDocumentService documentService,
            UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _documentService = documentService;
            _userManager = userManager;
        }

        public string StudentName { get; set; } = "Student";
        public int TotalCourses { get; set; }
        public int AvailableDocumentsCount { get; set; }
        public int ChatSessionsCount { get; set; }

        public List<Course> PopularCourses { get; set; } = [];
        public List<Document> RecentDocuments { get; set; } = [];
        public List<ChatConversationSummary> RecentChats { get; set; } = [];

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                StudentName = user.FullName ?? user.UserName ?? "Student";
            }

            // 1. Get courses list
            var courses = await _chatService.GetCoursesAsync();
            TotalCourses = courses.Count;
            PopularCourses = courses.Take(3).ToList(); // Get top 3 to display as popular

            // 2. Get approved documents count & list
            var docsResult = await _documentService.GetDocumentsAsync(searchTerm: null, currentUserId: null, isAdmin: false);
            var approvedDocs = docsResult.Documents
                .Where(d => d.Status == DocumentStatuses.Approved)
                .ToList();
            AvailableDocumentsCount = approvedDocs.Count;
            RecentDocuments = approvedDocs.Take(3).ToList(); // Recent 3 approved documents

            // 3. Get student's chat sessions
            var conversations = await _chatService.GetConversationSummariesAsync(userId);
            ChatSessionsCount = conversations.Count;
            RecentChats = conversations
                .Take(3)
                .ToList();
        }
    }
}
