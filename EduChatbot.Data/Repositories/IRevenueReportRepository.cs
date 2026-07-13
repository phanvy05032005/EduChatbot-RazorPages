using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EduChatbot.Models;

namespace EduChatbot.Data.Repositories;

public interface IRevenueReportRepository
{
    Task<AdminRevenueStatsDto> GetStatsAsync(DateTime now, RevenueReportFilterDto filter);
    Task<List<RevenueChartItemDto>> GetRevenueByMonthAsync(DateTime now);
    Task<List<PaymentStatusChartItemDto>> GetOrdersByStatusCountAsync(RevenueReportFilterDto filter);
    Task<List<RecentPaymentDto>> GetRecentPaidTransactionsAsync(int limit);
    Task<List<SubscriptionPlan>> GetSubscriptionPlansAsync();
}
