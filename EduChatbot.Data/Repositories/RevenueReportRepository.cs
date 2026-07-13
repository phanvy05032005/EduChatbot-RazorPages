using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EduChatbot.Models;
using EduChatbot.Models.Enums;

namespace EduChatbot.Data.Repositories;

public class RevenueReportRepository : IRevenueReportRepository
{
    private readonly ApplicationDbContext _context;

    public RevenueReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminRevenueStatsDto> GetStatsAsync(DateTime now, RevenueReportFilterDto filter)
    {
        var todayStart = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearStart = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Base query for SUCCESS transactions (for revenue calculations)
        var revQuery = _context.PaymentTransactions
            .AsNoTracking()
            .Where(pt => pt.Status == PaymentStatus.SUCCESS && pt.PaidAt != null);

        // Apply plan filter to revenue queries if specified
        if (filter.SubscriptionPlanId.HasValue)
        {
            revQuery = revQuery.Where(pt => pt.Subscription != null && pt.Subscription.SubscriptionPlanId == filter.SubscriptionPlanId.Value);
        }

        // Apply date filters to the "TotalRevenue" if from/to date are provided
        var filteredRevQuery = revQuery;
        if (filter.FromDate.HasValue)
        {
            filteredRevQuery = filteredRevQuery.Where(pt => pt.PaidAt >= filter.FromDate.Value);
        }
        if (filter.ToDate.HasValue)
        {
            filteredRevQuery = filteredRevQuery.Where(pt => pt.PaidAt <= filter.ToDate.Value);
        }

        // Execute revenue sums
        var totalRevenue = await filteredRevQuery.SumAsync(pt => pt.Amount);
        
        var revenueToday = await revQuery
            .Where(pt => pt.PaidAt >= todayStart)
            .SumAsync(pt => pt.Amount);

        var revenueThisMonth = await revQuery
            .Where(pt => pt.PaidAt >= monthStart)
            .SumAsync(pt => pt.Amount);

        var revenueThisYear = await revQuery
            .Where(pt => pt.PaidAt >= yearStart)
            .SumAsync(pt => pt.Amount);

        // Base query for status counts (uses CreatedAt for date filters since non-paid transactions have PaidAt == null)
        var countQuery = _context.PaymentTransactions.AsNoTracking();

        if (filter.SubscriptionPlanId.HasValue)
        {
            countQuery = countQuery.Where(pt => pt.Subscription != null && pt.Subscription.SubscriptionPlanId == filter.SubscriptionPlanId.Value);
        }
        if (filter.FromDate.HasValue)
        {
            countQuery = countQuery.Where(pt => pt.CreatedAt >= filter.FromDate.Value);
        }
        if (filter.ToDate.HasValue)
        {
            countQuery = countQuery.Where(pt => pt.CreatedAt <= filter.ToDate.Value);
        }

        // Status counts
        var paidOrders = await countQuery.CountAsync(pt => pt.Status == PaymentStatus.SUCCESS);
        var pendingOrders = await countQuery.CountAsync(pt => pt.Status == PaymentStatus.PENDING);
        var cancelledOrders = await countQuery.CountAsync(pt => pt.Status == PaymentStatus.CANCELLED);
        var failedOrders = await countQuery.CountAsync(pt => pt.Status == PaymentStatus.FAILED);

        // Active Premium Students count (Premium plan & active subscription)
        var activePremium = await _context.Subscriptions
            .AsNoTracking()
            .CountAsync(s => s.Plan.Name == "Premium" && s.Status == SubscriptionStatus.ACTIVE && s.EndDate > now);

        // Expired Premium Subscriptions count (Premium plan & expired/ended subscription)
        var expiredPremium = await _context.Subscriptions
            .AsNoTracking()
            .CountAsync(s => s.Plan.Name == "Premium" && (s.Status == SubscriptionStatus.EXPIRED || (s.EndDate <= now && s.Status == SubscriptionStatus.ACTIVE)));

        return new AdminRevenueStatsDto
        {
            TotalRevenue = totalRevenue,
            RevenueToday = revenueToday,
            RevenueThisMonth = revenueThisMonth,
            RevenueThisYear = revenueThisYear,
            TotalPaidOrders = paidOrders,
            TotalPendingOrders = pendingOrders,
            TotalCancelledOrders = cancelledOrders,
            TotalFailedOrders = failedOrders,
            ActivePremiumStudents = activePremium,
            ExpiredPremiumSubscriptions = expiredPremium
        };
    }

    public async Task<List<RevenueChartItemDto>> GetRevenueByMonthAsync(DateTime now)
    {
        // Get paid transactions in the last 12 months
        var twelveMonthsAgo = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-11);

        var transactions = await _context.PaymentTransactions
            .AsNoTracking()
            .Where(pt => pt.Status == PaymentStatus.SUCCESS && pt.PaidAt != null && pt.PaidAt >= twelveMonthsAgo)
            .Select(pt => new { pt.Amount, pt.PaidAt })
            .ToListAsync();

        // Group in memory to format keys stably
        var grouped = transactions
            .GroupBy(pt => new { pt.PaidAt!.Value.Year, pt.PaidAt.Value.Month })
            .Select(g => new RevenueChartItemDto
            {
                MonthLabel = $"{g.Key.Year}-{g.Key.Month:D2}",
                Revenue = g.Sum(pt => pt.Amount)
            })
            .OrderBy(dto => dto.MonthLabel)
            .ToList();

        // Fill in missing months to ensure the chart always shows 12 continuous months
        var result = new List<RevenueChartItemDto>();
        for (int i = 0; i < 12; i++)
        {
            var m = twelveMonthsAgo.AddMonths(i);
            var label = $"{m.Year}-{m.Month:D2}";
            var match = grouped.FirstOrDefault(g => g.MonthLabel == label);
            result.Add(new RevenueChartItemDto
            {
                MonthLabel = label,
                Revenue = match?.Revenue ?? 0m
            });
        }

        return result;
    }

    public async Task<List<PaymentStatusChartItemDto>> GetOrdersByStatusCountAsync(RevenueReportFilterDto filter)
    {
        var query = _context.PaymentTransactions.AsNoTracking();

        if (filter.SubscriptionPlanId.HasValue)
        {
            query = query.Where(pt => pt.Subscription != null && pt.Subscription.SubscriptionPlanId == filter.SubscriptionPlanId.Value);
        }
        if (filter.FromDate.HasValue)
        {
            query = query.Where(pt => pt.CreatedAt >= filter.FromDate.Value);
        }
        if (filter.ToDate.HasValue)
        {
            query = query.Where(pt => pt.CreatedAt <= filter.ToDate.Value);
        }

        var groups = await query
            .GroupBy(pt => pt.Status)
            .Select(g => new PaymentStatusChartItemDto
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync();

        return groups;
    }

    public async Task<List<RecentPaymentDto>> GetRecentPaidTransactionsAsync(int limit)
    {
        return await _context.PaymentTransactions
            .AsNoTracking()
            .Where(pt => pt.Status == PaymentStatus.SUCCESS && pt.PaidAt != null)
            .OrderByDescending(pt => pt.PaidAt)
            .Take(limit)
            .Select(pt => new RecentPaymentDto
            {
                StudentName = pt.User.FullName,
                StudentEmail = pt.User.Email ?? string.Empty,
                PackageName = pt.Subscription != null ? pt.Subscription.Plan.Name : "Unknown",
                Amount = pt.Amount,
                Status = "Paid",
                CreatedAt = pt.CreatedAt,
                PaidAt = pt.PaidAt
            })
            .ToListAsync();
    }

    public async Task<List<SubscriptionPlan>> GetSubscriptionPlansAsync()
    {
        return await _context.SubscriptionPlans
            .AsNoTracking()
            .OrderBy(p => p.Price)
            .ToListAsync();
    }
}
