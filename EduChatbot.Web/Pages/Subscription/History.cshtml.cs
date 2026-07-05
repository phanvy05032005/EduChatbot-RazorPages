using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Subscription;

[Authorize(Roles = ApplicationRoles.Student)]
public class HistoryModel : PageModel
{
    private const int DefaultPageSize = 10;

    private readonly IPaymentHistoryService _paymentHistoryService;

    public HistoryModel(IPaymentHistoryService paymentHistoryService)
    {
        _paymentHistoryService = paymentHistoryService;
    }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Sort { get; set; }

    public PagedResult<StudentPaymentHistoryItemViewModel> History { get; private set; } =
        new()
        {
            Page = 1,
            PageSize = DefaultPageSize
        };

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        History = await _paymentHistoryService.GetStudentHistoryAsync(userId, PageNumber, DefaultPageSize, Sort);
        return Page();
    }
}
