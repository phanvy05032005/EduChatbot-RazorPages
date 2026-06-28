using System;
using System.Linq;
using System.Threading.Tasks;
using EduChatbot.Data;
using EduChatbot.Models;
using EduChatbot.Models.Enums;
using EduChatbot.Models.Identity;
using EduChatbot.Business.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduChatbot.Business.Services;

public class SubscriptionAccessService : ISubscriptionAccessService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SubscriptionAccessService> _logger;

    public SubscriptionAccessService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<SubscriptionAccessService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task EnsureBasicSubscriptionAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // Check if there is any active or suspended Basic subscription for this user
        var hasBasic = await _context.Subscriptions
            .Include(s => s.Plan)
            .AnyAsync(s => s.UserId == userId && s.Plan.Name == "Basic");

        if (!hasBasic)
        {
            var basicPlan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Name == "Basic")
                ?? throw new InvalidOperationException("Basic plan is missing from database.");

            // Create suspended Basic subscription if the user is already premium
            var status = user.SubscriptionType == SubscriptionType.PREMIUM 
                ? SubscriptionStatus.SUSPENDED 
                : SubscriptionStatus.ACTIVE;

            _logger.LogInformation("Creating default Basic subscription for user {UserId} with status {Status}.", userId, status);

            var subscription = new Subscription
            {
                UserId = userId,
                SubscriptionPlanId = basicPlan.Id,
                Status = status,
                StartDate = DateTime.UtcNow,
                EndDate = null,
                RemainingRequests = basicPlan.RequestLimit,
                RequestWindowStart = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Subscription?> GetCurrentSubscriptionAsync(string userId)
    {
        var allSubs = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId)
            .ToListAsync();

        _logger.LogInformation("GetCurrentSubscriptionAsync for user {UserId}. Subscriptions in DB: {Subs}", 
            userId, 
            string.Join("; ", allSubs.Select(s => $"{s.Plan.Name}: ID={s.Id}, Status={s.Status}, Remaining={s.RemainingRequests}, WindowStart={s.RequestWindowStart}")));

        // Prioritize ACTIVE Premium first
        var premium = allSubs.FirstOrDefault(s => s.Plan.Name == "Premium" && s.Status == SubscriptionStatus.ACTIVE);
        if (premium != null)
        {
            _logger.LogInformation("Resolved active Premium subscription: ID={SubId}, Remaining={Remaining}", premium.Id, premium.RemainingRequests);
            return premium;
        }

        // Fallback to ACTIVE Basic
        var basic = allSubs.FirstOrDefault(s => s.Plan.Name == "Basic" && s.Status == SubscriptionStatus.ACTIVE);
        if (basic != null)
        {
            _logger.LogInformation("Resolved active Basic subscription: ID={SubId}, Remaining={Remaining}", basic.Id, basic.RemainingRequests);
            return basic;
        }

        _logger.LogWarning("No active Premium or Basic subscription found for user {UserId}.", userId);
        return null;
    }

    public async Task ReconcileExpiredSubscriptionsAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return;

        // Find if user has an ACTIVE Premium subscription that has expired
        var now = DateTime.UtcNow;
        var expiredPremium = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Plan.Name == "Premium" && s.Status == SubscriptionStatus.ACTIVE && s.EndDate != null && now > s.EndDate.Value);

        if (expiredPremium != null)
        {
            _logger.LogInformation("Found expired Premium subscription ID={SubId} for user {UserId}. EndDate={End}, Now={Now}. Reverting to Basic.",
                expiredPremium.Id, userId, expiredPremium.EndDate, now);

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Mark Premium as EXPIRED
                expiredPremium.Status = SubscriptionStatus.EXPIRED;
                expiredPremium.UpdatedAt = now;

                // 2. Reactivate Basic
                var basicPlan = await _context.SubscriptionPlans
                    .FirstOrDefaultAsync(p => p.Name == "Basic")
                    ?? throw new InvalidOperationException("Basic plan is missing from database.");

                var basicSub = await _context.Subscriptions
                    .FirstOrDefaultAsync(s => s.UserId == userId && s.SubscriptionPlanId == basicPlan.Id);

                if (basicSub != null)
                {
                    basicSub.Status = SubscriptionStatus.ACTIVE;
                    basicSub.RemainingRequests = basicPlan.RequestLimit;
                    basicSub.RequestWindowStart = now;
                    basicSub.UpdatedAt = now;
                }
                else
                {
                    // Create new active Basic if somehow missing
                    var newBasicSub = new Subscription
                    {
                        UserId = userId,
                        SubscriptionPlanId = basicPlan.Id,
                        Status = SubscriptionStatus.ACTIVE,
                        StartDate = now,
                        EndDate = null,
                        RemainingRequests = basicPlan.RequestLimit,
                        RequestWindowStart = now,
                        CreatedAt = now
                    };
                    _context.Subscriptions.Add(newBasicSub);
                }

                // 3. Revert user compatibility fields
                user.SubscriptionType = SubscriptionType.BASIC;
                user.AccountTitle = "Basic";
                user.TokenLimit = 5000;
                await _userManager.UpdateAsync(user);

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconcile expired subscription for user {UserId}", userId);
                await dbTransaction.RollbackAsync();
                throw;
            }
        }
    }

    public async Task CheckCanChatAsync(string userId)
    {
        await ReconcileExpiredSubscriptionsAsync(userId);
        await EnsureBasicSubscriptionAsync(userId);

        var currentSub = await GetCurrentSubscriptionAsync(userId)
            ?? throw new InvalidOperationException("Không tìm thấy gói đang hoạt động. Vui lòng liên hệ quản trị viên.");

        var plan = currentSub.Plan;

        if (!plan.AllowChat)
        {
            throw new InvalidOperationException("Gói hiện tại không cho phép sử dụng Chat.");
        }

        var now = DateTime.UtcNow;
        // Ensure both dates are UTC kind before subtraction to avoid local timezone offset comparison issues
        var windowStart = DateTime.SpecifyKind(currentSub.RequestWindowStart, DateTimeKind.Utc);
        var diff = now - windowStart;

        _logger.LogInformation("Quota check: userId={UserId}, subId={SubId}, planName={PlanName}, remaining={Remaining}, windowStart={Start}, now={Now}, diffMinutes={Diff}, refreshInterval={Interval}",
            userId, currentSub.Id, plan.Name, currentSub.RemainingRequests, windowStart, now, diff.TotalMinutes, plan.RefreshIntervalMinutes);

        // Lazy refresh request quota
        if (diff >= TimeSpan.FromMinutes(plan.RefreshIntervalMinutes))
        {
            _logger.LogInformation("Refreshing request quota for subId={SubId} from {OldRemaining} to {NewLimit}", currentSub.Id, currentSub.RemainingRequests, plan.RequestLimit);
            currentSub.RemainingRequests = plan.RequestLimit;
            currentSub.RequestWindowStart = now;
            currentSub.UpdatedAt = now;
            await _context.SaveChangesAsync();
        }

        if (currentSub.RemainingRequests <= 0)
        {
            throw new QuotaExceededException("Bạn đã hết lượt hỏi trong chu kỳ hiện tại. Vui lòng chờ lượt được làm mới hoặc nâng cấp gói.");
        }
    }

    public async Task ConsumeChatRequestAsync(string userId)
    {
        var currentSub = await GetCurrentSubscriptionAsync(userId);
        if (currentSub != null)
        {
            _logger.LogInformation("Consuming request: userId={UserId}, subId={SubId}, planName={PlanName}, oldRemaining={Old}",
                userId, currentSub.Id, currentSub.Plan.Name, currentSub.RemainingRequests);

            if (currentSub.RemainingRequests > 0)
            {
                currentSub.RemainingRequests -= 1;
                currentSub.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Consumed request successfully: subId={SubId}, newRemaining={New}",
                    currentSub.Id, currentSub.RemainingRequests);
            }
            else
            {
                _logger.LogWarning("Cannot consume request: subId={SubId} remaining requests is already 0.", currentSub.Id);
            }
        }
        else
        {
            _logger.LogWarning("Cannot consume request: no active subscription resolved for user {UserId}.", userId);
        }
    }

    public async Task<bool> CheckCanUseQuizAsync(string userId)
    {
        await ReconcileExpiredSubscriptionsAsync(userId);
        await EnsureBasicSubscriptionAsync(userId);

        var currentSub = await GetCurrentSubscriptionAsync(userId);
        return currentSub?.Plan.AllowQuiz ?? false;
    }
}
