using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.MVC.Pages.Documents;

[Authorize(Roles = ApplicationRoles.DocumentManagers)]
public class IndexModel : PageModel
{
    private readonly IDocumentService _documentService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public IndexModel(IDocumentService documentService, IWebHostEnvironment webHostEnvironment)
    {
        _documentService = documentService;
        _webHostEnvironment = webHostEnvironment;
    }

    public DocumentListResult Result { get; private set; } = new();

    public async Task OnGetAsync(string? searchTerm)
    {
        Result = await _documentService.GetDocumentsAsync(
            searchTerm,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole(ApplicationRoles.Admin));
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
}
