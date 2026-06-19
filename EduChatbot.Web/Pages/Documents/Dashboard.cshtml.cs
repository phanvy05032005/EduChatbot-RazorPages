using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Microsoft.AspNetCore.Mvc;

namespace EduChatbot.Web.Pages.Documents;

[Authorize(Roles = ApplicationRoles.DocumentManagers)]
public class DashboardModel : PageModel
{
    private readonly IDocumentService _documentService;

    public DashboardModel(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public DocumentDashboardSummary Summary { get; private set; } = new();

    public List<Course> AssignedCourses { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Summary = await _documentService.GetDashboardSummaryAsync();

        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(lecturerId))
        {
            AssignedCourses = await _documentService.GetAvailableCoursesForUserAsync(
                lecturerId, User.IsInRole(ApplicationRoles.Admin));
        }
    }
}
