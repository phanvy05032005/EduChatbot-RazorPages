using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Student
{
    [Authorize(Roles = ApplicationRoles.Student)]
    public class CourseDetailModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly IDocumentService _documentService;

        public CourseDetailModel(IChatService chatService, IDocumentService documentService)
        {
            _chatService = chatService;
            _documentService = documentService;
        }

        public Course? Course { get; private set; }
        public List<Document> Documents { get; private set; } = [];

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var courses = await _chatService.GetCoursesAsync();
            Course = courses.FirstOrDefault(c => c.Id == id);

            if (Course == null)
            {
                return NotFound();
            }

            // Load approved documents for this course
            var docsResult = await _documentService.GetDocumentsAsync(searchTerm: null, currentUserId: null, isAdmin: false, courseId: id);
            Documents = docsResult.Documents
                .Where(d => d.Status == DocumentStatuses.Approved)
                .ToList();

            return Page();
        }
    }
}
