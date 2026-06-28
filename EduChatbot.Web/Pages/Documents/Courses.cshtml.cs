using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Documents;

[Authorize(Roles = ApplicationRoles.DocumentManagers)]
public class CoursesModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly IDocumentService _documentService;
    private readonly ILecturerQuizService _lecturerQuizService;

    public CoursesModel(
        IAdminService adminService,
        IDocumentService documentService,
        ILecturerQuizService lecturerQuizService)
    {
        _adminService = adminService;
        _documentService = documentService;
        _lecturerQuizService = lecturerQuizService;
    }

    public List<Course> Courses { get; private set; } = [];
    public List<Document> Documents { get; private set; } = [];
    public List<Quiz> Quizzes { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Courses = await _adminService.GetCoursesAsync();
        
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(currentUserId))
        {
            var docResult = await _documentService.GetDocumentsAsync(currentUserId: currentUserId);
            Documents = docResult?.Documents ?? [];
            
            Quizzes = await _lecturerQuizService.GetLecturerQuizzesAsync(currentUserId);
        }
    }
}
