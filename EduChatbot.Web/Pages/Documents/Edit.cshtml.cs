using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Web.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Documents;

[Authorize(Roles = ApplicationRoles.DocumentManagers)]
public class EditModel : PageModel
{
    private readonly IDocumentService _documentService;

    public EditModel(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [BindProperty]
    public DocumentEditViewModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var document = await _documentService.GetDocumentDetailsAsync(
            id,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole(ApplicationRoles.Admin));
        if (document == null)
        {
            return NotFound();
        }

        Input = new DocumentEditViewModel
        {
            Id = document.Id,
            FileName = document.FileName,
            StoredFileName = document.StoredFileName,
            FilePath = document.FilePath
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _documentService.UpdateDocumentAsync(
            Input.Id,
            Input.FileName,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole(ApplicationRoles.Admin));

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        TempData["UploadMessage"] = result.Message;
        TempData["ChunkCount"] = result.ChunkCount;
        TempData["IndexStatus"] = result.Status;

        return RedirectToPage("/Documents/Details", new { id = Input.Id });
    }
}
