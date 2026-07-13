using System.Collections.Generic;
using System.Threading.Tasks;
using EduChatbot.Models;

namespace EduChatbot.Business.Services;

public interface IRevenueReportService
{
    Task<AdminRevenueStatsDto> GetStatsAsync(RevenueReportFilterDto filter);
    Task<List<RevenueChartItemDto>> GetRevenueByMonthAsync();
    Task<List<PaymentStatusChartItemDto>> GetOrdersByStatusCountAsync(RevenueReportFilterDto filter);
    Task<List<RecentPaymentDto>> GetRecentPaidTransactionsAsync(int limit = 10);
    Task<List<SubscriptionPlan>> GetSubscriptionPlansAsync();
    Task<string> GetVerificationReportAsync();
}
