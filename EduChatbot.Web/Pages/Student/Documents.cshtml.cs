using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Student
{
    [Authorize(Roles = ApplicationRoles.Student)]
    public class DocumentsModel : PageModel
    {
        private readonly IDocumentService _documentService;
        private readonly IChatService _chatService;

        public DocumentsModel(IDocumentService documentService, IChatService chatService)
        {
            _documentService = documentService;
            _chatService = chatService;
        }

        public List<Document> Documents { get; private set; } = [];
        public List<Course> Courses { get; private set; } = [];

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CourseId { get; set; }

        public long TotalSpaceUsed { get; private set; }

        public async Task OnGetAsync()
        {
            // Load courses for filter dropdown
            Courses = await _chatService.GetCoursesAsync();

            // Load approved documents based on search and course filter
            var docsResult = await _documentService.GetDocumentsAsync(SearchTerm, currentUserId: null, isAdmin: false, courseId: CourseId);
            Documents = docsResult.Documents
                .Where(d => d.Status == DocumentStatuses.Approved)
                .ToList();

            // Compute total space used in bytes
            TotalSpaceUsed = Documents.Sum(d => d.FileSize);
        }
    }
}
