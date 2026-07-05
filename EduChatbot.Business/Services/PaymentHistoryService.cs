using EduChatbot.Data;
using EduChatbot.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduChatbot.Business.Services;

public class PaymentHistoryService : IPaymentHistoryService
{
    private readonly ApplicationDbContext _context;

    public PaymentHistoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<StudentPaymentHistoryItemViewModel>> GetStudentHistoryAsync(
        string userId,
        int page,
        int pageSize,
        string? sort)
    {
        var normalizedPage = NormalizePage(page);
        var normalizedPageSize = NormalizePageSize(pageSize, 10, 50);
        var now = DateTime.UtcNow;

        var query = _context.PaymentTransactions
            .AsNoTracking()
            .Where(pt => pt.UserId == userId)
            .Select(pt => new PaymentHistoryProjection
            {
                StudentName = pt.User.FullName,
                StudentEmail = pt.User.Email ?? string.Empty,
                PackageName = pt.Subscription != null ? pt.Subscription.Plan.Name : "Unknown",
                Amount = pt.Amount,
                Currency = pt.Currency,
                Provider = pt.Provider,
                OrderCode = pt.OrderCode,
                ProviderTransactionCode = pt.ProviderTransactionCode,
                ProviderReference = pt.PayOSPaymentLinkId,
                RawStatus = pt.Status,
                CreatedAt = pt.CreatedAt,
                PaidAt = pt.PaidAt,
                ExpiredAt = pt.Subscription != null ? pt.Subscription.EndDate : null,
                SubscriptionStatus = pt.Subscription != null ? pt.Subscription.Status : null,
                StatusReason = pt.StatusReason
            });

        query = ApplySort(query, sort, isAdmin: false);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        return new PagedResult<StudentPaymentHistoryItemViewModel>
        {
            Items = items.Select(item => MapStudentItem(item, now)).ToList(),
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<AdminPaymentHistoryItemViewModel>> GetAdminHistoryAsync(
        PaymentHistoryAdminFilter filter,
        int page,
        int pageSize,
        string? sort)
    {
        var normalizedPage = NormalizePage(page);
        var normalizedPageSize = NormalizePageSize(pageSize, 20, 100);
        var now = DateTime.UtcNow;

        var query = BuildAdminProjectionQuery(filter, now);
        query = ApplySort(query, sort, isAdmin: true);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync();

        return new PagedResult<AdminPaymentHistoryItemViewModel>
        {
            Items = items.Select(item => MapAdminItem(item, now)).ToList(),
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PaymentHistoryAdminSummaryViewModel> GetAdminHistorySummaryAsync(PaymentHistoryAdminFilter filter)
    {
        var now = DateTime.UtcNow;
        var query = BuildAdminProjectionQuery(filter, now);

        return new PaymentHistoryAdminSummaryViewModel
        {
            TotalTransactions = await query.CountAsync(),
            PaidTransactions = await query.CountAsync(item => item.RawStatus == PaymentStatus.SUCCESS),
            TotalRevenue = await query
                .Where(item => item.RawStatus == PaymentStatus.SUCCESS && item.PaidAt.HasValue)
                .SumAsync(item => (decimal?)item.Amount) ?? 0m,
            PendingTransactions = await query.CountAsync(item => item.RawStatus == PaymentStatus.PENDING),
            FailedOrCancelledTransactions = await query.CountAsync(item =>
                item.RawStatus == PaymentStatus.FAILED || item.RawStatus == PaymentStatus.CANCELLED)
        };
    }

    private IQueryable<PaymentHistoryProjection> BuildAdminProjectionQuery(PaymentHistoryAdminFilter filter, DateTime now)
    {
        var query = _context.PaymentTransactions
            .AsNoTracking()
            .Select(pt => new PaymentHistoryProjection
            {
                StudentName = pt.User.FullName,
                StudentEmail = pt.User.Email ?? string.Empty,
                PackageName = pt.Subscription != null ? pt.Subscription.Plan.Name : "Unknown",
                Amount = pt.Amount,
                Currency = pt.Currency,
                Provider = pt.Provider,
                OrderCode = pt.OrderCode,
                ProviderTransactionCode = pt.ProviderTransactionCode,
                ProviderReference = pt.PayOSPaymentLinkId,
                RawStatus = pt.Status,
                CreatedAt = pt.CreatedAt,
                PaidAt = pt.PaidAt,
                ExpiredAt = pt.Subscription != null ? pt.Subscription.EndDate : null,
                SubscriptionStatus = pt.Subscription != null ? pt.Subscription.Status : null,
                StatusReason = pt.StatusReason
            });

        query = ApplyAdminFilters(query, filter, now);
        return query;
    }

    private static IQueryable<PaymentHistoryProjection> ApplyAdminFilters(
        IQueryable<PaymentHistoryProjection> query,
        PaymentHistoryAdminFilter filter,
        DateTime now)
    {
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim().ToLower();
            query = query.Where(item =>
                item.StudentName.ToLower().Contains(searchTerm)
                || item.StudentEmail.ToLower().Contains(searchTerm)
                || item.PackageName.ToLower().Contains(searchTerm)
                || item.OrderCode.ToString().Contains(searchTerm)
                || (item.ProviderTransactionCode != null && item.ProviderTransactionCode.ToLower().Contains(searchTerm))
                || (item.ProviderReference != null && item.ProviderReference.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(filter.StudentName))
        {
            var studentName = filter.StudentName.Trim().ToLower();
            query = query.Where(item => item.StudentName.ToLower().Contains(studentName));
        }

        if (!string.IsNullOrWhiteSpace(filter.StudentEmail))
        {
            var studentEmail = filter.StudentEmail.Trim().ToLower();
            query = query.Where(item => item.StudentEmail.ToLower().Contains(studentEmail));
        }

        if (!string.IsNullOrWhiteSpace(filter.OrderCode))
        {
            var orderCode = filter.OrderCode.Trim();
            query = query.Where(item => item.OrderCode.ToString().Contains(orderCode));
        }

        if (!string.IsNullOrWhiteSpace(filter.PackageName))
        {
            var packageName = filter.PackageName.Trim().ToLower();
            query = query.Where(item => item.PackageName.ToLower().Contains(packageName));
        }

        if (!string.IsNullOrWhiteSpace(filter.PaymentMethod))
        {
            if (Enum.TryParse<PaymentProvider>(filter.PaymentMethod, true, out var paymentProvider))
            {
                query = query.Where(item => item.Provider == paymentProvider);
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.RawPaymentStatus)
            && Enum.TryParse<PaymentStatus>(filter.RawPaymentStatus, true, out var rawStatus))
        {
            query = query.Where(item => item.RawStatus == rawStatus);
        }

        if (!string.IsNullOrWhiteSpace(filter.DisplayStatus))
        {
            var normalizedStatus = filter.DisplayStatus.Trim().ToLowerInvariant();
            query = normalizedStatus switch
            {
                "pending" => query.Where(item => item.RawStatus == PaymentStatus.PENDING),
                "failed" => query.Where(item => item.RawStatus == PaymentStatus.FAILED),
                "cancelled" => query.Where(item => item.RawStatus == PaymentStatus.CANCELLED),
                "expired" => query.Where(item =>
                    item.RawStatus == PaymentStatus.SUCCESS
                    && (item.SubscriptionStatus == SubscriptionStatus.EXPIRED
                        || (item.ExpiredAt.HasValue && item.ExpiredAt.Value < now))),
                "paid" => query.Where(item =>
                    item.RawStatus == PaymentStatus.SUCCESS
                    && item.SubscriptionStatus != SubscriptionStatus.EXPIRED
                    && (!item.ExpiredAt.HasValue || item.ExpiredAt.Value >= now)),
                _ => query
            };
        }

        var createdFrom = NormalizeStartDate(filter.CreatedFrom);
        var createdToExclusive = NormalizeExclusiveEndDate(filter.CreatedTo);
        var paidFrom = NormalizeStartDate(filter.PaidFrom);
        var paidToExclusive = NormalizeExclusiveEndDate(filter.PaidTo);
        var expiredFrom = NormalizeStartDate(filter.ExpiredFrom);
        var expiredToExclusive = NormalizeExclusiveEndDate(filter.ExpiredTo);

        if (TryResolvePresetRange(filter.PresetRange, now, out var presetStart, out var presetEndExclusive))
        {
            createdFrom = MaxDate(createdFrom, presetStart);
            createdToExclusive = MinDate(createdToExclusive, presetEndExclusive);
        }

        if (createdFrom.HasValue)
        {
            query = query.Where(item => item.CreatedAt >= createdFrom.Value);
        }

        if (createdToExclusive.HasValue)
        {
            query = query.Where(item => item.CreatedAt < createdToExclusive.Value);
        }

        if (paidFrom.HasValue)
        {
            query = query.Where(item => item.PaidAt.HasValue && item.PaidAt.Value >= paidFrom.Value);
        }

        if (paidToExclusive.HasValue)
        {
            query = query.Where(item => item.PaidAt.HasValue && item.PaidAt.Value < paidToExclusive.Value);
        }

        if (expiredFrom.HasValue)
        {
            query = query.Where(item => item.ExpiredAt.HasValue && item.ExpiredAt.Value >= expiredFrom.Value);
        }

        if (expiredToExclusive.HasValue)
        {
            query = query.Where(item => item.ExpiredAt.HasValue && item.ExpiredAt.Value < expiredToExclusive.Value);
        }

        return query;
    }

    private static IQueryable<PaymentHistoryProjection> ApplySort(
        IQueryable<PaymentHistoryProjection> query,
        string? sort,
        bool isAdmin)
    {
        var normalizedSort = sort?.Trim().ToLowerInvariant();

        return normalizedSort switch
        {
            "created_asc" => query.OrderBy(item => item.CreatedAt).ThenBy(item => item.OrderCode),
            "paid_desc" => query.OrderByDescending(item => item.PaidAt ?? DateTime.MinValue).ThenByDescending(item => item.CreatedAt),
            "paid_asc" => query.OrderBy(item => item.PaidAt ?? DateTime.MaxValue).ThenBy(item => item.CreatedAt),
            "amount_desc" => query.OrderByDescending(item => item.Amount).ThenByDescending(item => item.CreatedAt),
            "amount_asc" => query.OrderBy(item => item.Amount).ThenByDescending(item => item.CreatedAt),
            "student_asc" when isAdmin => query.OrderBy(item => item.StudentName).ThenByDescending(item => item.CreatedAt),
            "student_desc" when isAdmin => query.OrderByDescending(item => item.StudentName).ThenByDescending(item => item.CreatedAt),
            "package_asc" when isAdmin => query.OrderBy(item => item.PackageName).ThenByDescending(item => item.CreatedAt),
            "package_desc" when isAdmin => query.OrderByDescending(item => item.PackageName).ThenByDescending(item => item.CreatedAt),
            _ => query.OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.OrderCode)
        };
    }

    private static StudentPaymentHistoryItemViewModel MapStudentItem(PaymentHistoryProjection item, DateTime now)
    {
        return new StudentPaymentHistoryItemViewModel
        {
            CreatedAt = item.CreatedAt,
            PackageName = item.PackageName,
            Amount = item.Amount,
            Currency = item.Currency,
            PaymentMethod = item.Provider.ToString(),
            OrderCode = item.OrderCode,
            DisplayStatus = ResolveDisplayStatus(item, now),
            RawPaymentStatus = item.RawStatus.ToString(),
            PaidAt = item.PaidAt,
            ExpiredAt = item.ExpiredAt,
            StatusReason = item.StatusReason
        };
    }

    private static AdminPaymentHistoryItemViewModel MapAdminItem(PaymentHistoryProjection item, DateTime now)
    {
        return new AdminPaymentHistoryItemViewModel
        {
            StudentName = item.StudentName,
            StudentEmail = item.StudentEmail,
            PackageName = item.PackageName,
            Amount = item.Amount,
            Currency = item.Currency,
            PaymentMethod = item.Provider.ToString(),
            OrderCode = item.OrderCode,
            ProviderTransactionCode = item.ProviderTransactionCode,
            ProviderReference = item.ProviderReference,
            DisplayStatus = ResolveDisplayStatus(item, now),
            RawPaymentStatus = item.RawStatus.ToString(),
            CreatedAt = item.CreatedAt,
            PaidAt = item.PaidAt,
            ExpiredAt = item.ExpiredAt,
            StatusReason = item.StatusReason
        };
    }

    private static string ResolveDisplayStatus(PaymentHistoryProjection item, DateTime now)
    {
        return item.RawStatus switch
        {
            PaymentStatus.PENDING => "Pending",
            PaymentStatus.FAILED => "Failed",
            PaymentStatus.CANCELLED => "Cancelled",
            PaymentStatus.SUCCESS when IsExpiredSuccessfulPayment(item, now) => "Expired",
            PaymentStatus.SUCCESS => "Paid",
            _ => item.RawStatus.ToString()
        };
    }

    private static bool IsExpiredSuccessfulPayment(PaymentHistoryProjection item, DateTime now)
    {
        if (item.RawStatus != PaymentStatus.SUCCESS)
        {
            return false;
        }

        if (item.SubscriptionStatus == SubscriptionStatus.EXPIRED)
        {
            return true;
        }

        return item.ExpiredAt.HasValue && item.ExpiredAt.Value < now;
    }

    private static DateTime? NormalizeStartDate(DateTime? value)
    {
        return value?.Date;
    }

    private static DateTime? NormalizeExclusiveEndDate(DateTime? value)
    {
        return value?.Date.AddDays(1);
    }

    private static bool TryResolvePresetRange(
        string? presetRange,
        DateTime now,
        out DateTime start,
        out DateTime endExclusive)
    {
        start = default;
        endExclusive = default;

        if (string.IsNullOrWhiteSpace(presetRange))
        {
            return false;
        }

        var today = now.Date;
        switch (presetRange.Trim().ToLowerInvariant())
        {
            case "today":
                start = today;
                endExclusive = today.AddDays(1);
                return true;
            case "thisweek":
                var diff = ((int)today.DayOfWeek + 6) % 7;
                start = today.AddDays(-diff);
                endExclusive = start.AddDays(7);
                return true;
            case "thismonth":
                start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                endExclusive = start.AddMonths(1);
                return true;
            case "last30days":
                start = today.AddDays(-29);
                endExclusive = today.AddDays(1);
                return true;
            default:
                return false;
        }
    }

    private static DateTime? MaxDate(DateTime? left, DateTime? right)
    {
        if (!left.HasValue)
        {
            return right;
        }

        if (!right.HasValue)
        {
            return left;
        }

        return left > right ? left : right;
    }

    private static DateTime? MinDate(DateTime? left, DateTime? right)
    {
        if (!left.HasValue)
        {
            return right;
        }

        if (!right.HasValue)
        {
            return left;
        }

        return left < right ? left : right;
    }

    private static int NormalizePage(int page)
    {
        return page < 1 ? 1 : page;
    }

    private static int NormalizePageSize(int pageSize, int defaultSize, int maxSize)
    {
        if (pageSize <= 0)
        {
            return defaultSize;
        }

        return Math.Min(pageSize, maxSize);
    }

    private sealed class PaymentHistoryProjection
    {
        public string StudentName { get; init; } = string.Empty;

        public string StudentEmail { get; init; } = string.Empty;

        public string PackageName { get; init; } = string.Empty;

        public decimal Amount { get; init; }

        public string Currency { get; init; } = string.Empty;

        public PaymentProvider Provider { get; init; }

        public long OrderCode { get; init; }

        public string? ProviderTransactionCode { get; init; }

        public string? ProviderReference { get; init; }

        public PaymentStatus RawStatus { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime? PaidAt { get; init; }

        public DateTime? ExpiredAt { get; init; }

        public SubscriptionStatus? SubscriptionStatus { get; init; }

        public string? StatusReason { get; init; }
    }
}
