using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EduChatbot.Data.Repositories;
using EduChatbot.Models;

namespace EduChatbot.Business.Services;

public class RevenueReportService : IRevenueReportService
{
    private readonly IRevenueReportRepository _repository;

    public RevenueReportService(IRevenueReportRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminRevenueStatsDto> GetStatsAsync(RevenueReportFilterDto filter)
    {
        var now = DateTime.UtcNow;
        return await _repository.GetStatsAsync(now, filter);
    }

    public async Task<List<RevenueChartItemDto>> GetRevenueByMonthAsync()
    {
        var now = DateTime.UtcNow;
        return await _repository.GetRevenueByMonthAsync(now);
    }

    public async Task<List<PaymentStatusChartItemDto>> GetOrdersByStatusCountAsync(RevenueReportFilterDto filter)
    {
        return await _repository.GetOrdersByStatusCountAsync(filter);
    }

    public async Task<List<RecentPaymentDto>> GetRecentPaidTransactionsAsync(int limit = 10)
    {
        return await _repository.GetRecentPaidTransactionsAsync(limit);
    }

    public async Task<List<SubscriptionPlan>> GetSubscriptionPlansAsync()
    {
        return await _repository.GetSubscriptionPlansAsync();
    }

    public async Task<string> GetVerificationReportAsync()
    {
        var now = DateTime.UtcNow;
        var stats = await _repository.GetStatsAsync(now, new RevenueReportFilterDto());
        var monthly = await _repository.GetRevenueByMonthAsync(now);
        var statusCounts = await _repository.GetOrdersByStatusCountAsync(new RevenueReportFilterDto());

        var report = new System.Text.StringBuilder();
        report.AppendLine("=== REVENUE SERVICE DATA REPORT ===");
        report.AppendLine($"Total Revenue: {stats.TotalRevenue} VND");
        report.AppendLine($"Revenue Today: {stats.RevenueToday} VND");
        report.AppendLine($"Revenue This Month: {stats.RevenueThisMonth} VND");
        report.AppendLine($"Revenue This Year: {stats.RevenueThisYear} VND");
        report.AppendLine($"Paid Orders Count: {stats.TotalPaidOrders}");
        report.AppendLine($"Pending Orders Count: {stats.TotalPendingOrders}");
        report.AppendLine($"Cancelled Orders Count: {stats.TotalCancelledOrders}");
        report.AppendLine($"Failed Orders Count: {stats.TotalFailedOrders}");
        report.AppendLine($"Active Premium Students: {stats.ActivePremiumStudents}");
        report.AppendLine($"Expired Premium Subscriptions: {stats.ExpiredPremiumSubscriptions}");

        report.AppendLine("\nMonthly breakdown:");
        foreach (var m in monthly)
        {
            report.AppendLine($"  {m.MonthLabel}: {m.Revenue} VND");
        }

        report.AppendLine("\nStatus breakdown:");
        foreach (var sc in statusCounts)
        {
            report.AppendLine($"  {sc.Status}: {sc.Count}");
        }
        report.AppendLine("====================================");

        return report.ToString();
    }
}
