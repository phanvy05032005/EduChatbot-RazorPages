using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.MVC.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class PendingDocumentsModel : PageModel
{
    private readonly IDocumentService _documentService;

    public PendingDocumentsModel(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public List<Document> Documents { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Documents = await _documentService.GetPendingReviewDocumentsAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _documentService.ApproveDocumentAsync(id, adminId);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id, string? reviewNote)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _documentService.RejectDocumentAsync(id, adminId, reviewNote);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage();
    }
}
