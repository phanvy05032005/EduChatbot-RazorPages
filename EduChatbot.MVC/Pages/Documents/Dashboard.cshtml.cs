using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.MVC.Pages.Documents;

[Authorize(Roles = ApplicationRoles.DocumentManagers)]
public class DashboardModel : PageModel
{
    private readonly IDocumentService _documentService;

    public DashboardModel(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public DocumentDashboardSummary Summary { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Summary = await _documentService.GetDashboardSummaryAsync();
    }
}
