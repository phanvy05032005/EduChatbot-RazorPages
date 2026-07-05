using EduChatbot.Business.Services;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class PaymentHistoryModel : PageModel
{
    private const int DefaultPageSize = 20;

    private readonly IPaymentHistoryService _paymentHistoryService;

    public PaymentHistoryModel(IPaymentHistoryService paymentHistoryService)
    {
        _paymentHistoryService = paymentHistoryService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StudentName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StudentEmail { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? OrderCode { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? PackageName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? PaymentMethod { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RawPaymentStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? DisplayStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? CreatedFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? CreatedTo { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? PaidFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? PaidTo { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ExpiredFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ExpiredTo { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? PresetRange { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Sort { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedResult<AdminPaymentHistoryItemViewModel> History { get; private set; } =
        new()
        {
            Page = 1,
            PageSize = DefaultPageSize
        };

    public PaymentHistoryAdminSummaryViewModel Summary { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var filter = BuildFilter();
        History = await _paymentHistoryService.GetAdminHistoryAsync(filter, PageNumber, DefaultPageSize, Sort);
        Summary = await _paymentHistoryService.GetAdminHistorySummaryAsync(filter);
    }

    private PaymentHistoryAdminFilter BuildFilter()
    {
        return new PaymentHistoryAdminFilter
        {
            SearchTerm = SearchTerm,
            StudentName = StudentName,
            StudentEmail = StudentEmail,
            OrderCode = OrderCode,
            PackageName = PackageName,
            PaymentMethod = PaymentMethod,
            RawPaymentStatus = RawPaymentStatus,
            DisplayStatus = DisplayStatus,
            CreatedFrom = CreatedFrom,
            CreatedTo = CreatedTo,
            PaidFrom = PaidFrom,
            PaidTo = PaidTo,
            ExpiredFrom = ExpiredFrom,
            ExpiredTo = ExpiredTo,
            PresetRange = PresetRange
        };
    }
}
