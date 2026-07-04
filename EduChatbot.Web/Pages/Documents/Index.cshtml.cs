using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Documents;

[Authorize(Roles = ApplicationRoles.DocumentManagers)]
public class IndexModel : PageModel
{
    private readonly IDocumentService _documentService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IAdminService _adminService;

    public IndexModel(IDocumentService documentService, IWebHostEnvironment webHostEnvironment, IAdminService adminService)
    {
        _documentService = documentService;
        _webHostEnvironment = webHostEnvironment;
        _adminService = adminService;
    }

    public DocumentListResult Result { get; private set; } = new();

    public Course? FilteredCourse { get; private set; }

    public async Task OnGetAsync(string? searchTerm, int? courseId)
    {
        if (courseId.HasValue)
        {
            FilteredCourse = await _adminService.GetCourseByIdAsync(courseId.Value);
        }

        Result = await _documentService.GetDocumentsAsync(
            searchTerm,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole(ApplicationRoles.Admin),
            courseId);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var deleted = await _documentService.DeleteDocumentAsync(
            id,
            _webHostEnvironment.WebRootPath,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole(ApplicationRoles.Admin));

        TempData[deleted ? "UploadMessage" : "ErrorMessage"] = deleted
            ? "Document deleted successfully."
            : "Document not found.";

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDeleteImpactAsync(int id)
    {
        try
        {
            var impact = await _documentService.GetDeleteImpactAsync(
                id,
                User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                User.IsInRole(ApplicationRoles.Admin));

            return new JsonResult(impact);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostDeleteConfirmAsync(int id, string action)
    {
        var result = await _documentService.ExecuteDeleteOrArchiveAsync(
            id,
            action,
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            User.IsInRole(ApplicationRoles.Admin));

        return new JsonResult(new { success = result.IsSuccess, message = result.Message, action = result.ExecutedAction });
    }
}
