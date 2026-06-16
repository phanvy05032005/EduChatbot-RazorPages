using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Documents;

[Authorize(Roles = ApplicationRoles.DocumentManagers)]
public class DetailsModel : PageModel
{
    private readonly IDocumentService _documentService;

    public DetailsModel(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public Document? Document { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Document = await _documentService.GetDocumentDetailsAsync(
            id,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            User.IsInRole(ApplicationRoles.Admin));

        return Document == null ? NotFound() : Page();
    }
}
