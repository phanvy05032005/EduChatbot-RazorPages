using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Enums;
using EduChatbot.Models.Identity;

namespace EduChatbot.Web.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class RevenueReportModel : PageModel
{
    private readonly IRevenueReportService _revenueReportService;

    public RevenueReportModel(IRevenueReportService revenueReportService)
    {
        _revenueReportService = revenueReportService;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public PaymentStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SubscriptionPlanId { get; set; }

    public AdminRevenueStatsDto Stats { get; private set; } = new();
    public List<RevenueChartItemDto> MonthlyRevenue { get; private set; } = new();
    public List<PaymentStatusChartItemDto> StatusDistribution { get; private set; } = new();
    public List<RecentPaymentDto> RecentPayments { get; private set; } = new();
    public List<SubscriptionPlan> SubscriptionPlans { get; private set; } = new();

    public async Task OnGetAsync()
    {
        var filter = new RevenueReportFilterDto
        {
            FromDate = FromDate,
            ToDate = ToDate,
            Status = Status,
            SubscriptionPlanId = SubscriptionPlanId
        };

        Stats = await _revenueReportService.GetStatsAsync(filter);
        MonthlyRevenue = await _revenueReportService.GetRevenueByMonthAsync();
        StatusDistribution = await _revenueReportService.GetOrdersByStatusCountAsync(filter);
        RecentPayments = await _revenueReportService.GetRecentPaidTransactionsAsync(20);

        SubscriptionPlans = await _revenueReportService.GetSubscriptionPlansAsync();
    }
}
