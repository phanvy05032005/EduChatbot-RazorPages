using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduChatbot.Data;
using EduChatbot.Models.Enums;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduChatbot.Business.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISubscriptionAccessService _accessService;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ISubscriptionAccessService accessService,
        ILogger<SubscriptionService> logger)
    {
        _context = context;
        _userManager = userManager;
        _accessService = accessService;
        _logger = logger;
    }

    public async Task<List<SubscriptionPlanViewModel>> GetPlansAsync(string userId)
    {
        await _accessService.ReconcileExpiredSubscriptionsAsync(userId);
        await _accessService.EnsureBasicSubscriptionAsync(userId);

        var user = await _userManager.FindByIdAsync(userId);
        var currentType = user?.SubscriptionType ?? SubscriptionType.BASIC;

        var dbPlans = await _context.SubscriptionPlans
            .OrderBy(p => p.Price)
            .ToListAsync();

        var activeSub = await _accessService.GetCurrentSubscriptionAsync(userId);

        return dbPlans.Select(plan =>
        {
            var isCurrent = (plan.Name.Equals("Premium", StringComparison.OrdinalIgnoreCase) && currentType == SubscriptionType.PREMIUM) ||
                            (plan.Name.Equals("Basic", StringComparison.OrdinalIgnoreCase) && currentType == SubscriptionType.BASIC);

            return new SubscriptionPlanViewModel
            {
                Type = plan.Name.Equals("Premium", StringComparison.OrdinalIgnoreCase) ? nameof(SubscriptionType.PREMIUM) : nameof(SubscriptionType.BASIC),
                Name = plan.Name,
                Price = plan.Price,
                Currency = "VND",
                TokenLimit = plan.TokenLimit,
                QuizUnlocked = plan.AllowQuiz,
                Current = isCurrent,
                RequestLimit = plan.RequestLimit,
                RefreshIntervalMinutes = plan.RefreshIntervalMinutes,
                DurationDays = plan.DurationDays,
                RemainingRequests = isCurrent && activeSub != null ? activeSub.RemainingRequests : plan.RequestLimit
            };
        }).ToList();
    }

    public async Task<MySubscriptionViewModel> GetMySubscriptionAsync(string userId)
    {
        _logger.LogInformation("GetMySubscriptionAsync called for user {UserId}", userId);
        await _accessService.ReconcileExpiredSubscriptionsAsync(userId);
        await _accessService.EnsureBasicSubscriptionAsync(userId);

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        var activeSub = await _accessService.GetCurrentSubscriptionAsync(userId);
        if (activeSub == null)
        {
            _logger.LogWarning("GetMySubscriptionAsync: no active subscription resolved for user {UserId}.", userId);
            throw new InvalidOperationException("Không tìm thấy gói đang hoạt động. Vui lòng liên hệ quản trị viên.");
        }

        _logger.LogInformation("GetMySubscriptionAsync: resolved subId={SubId}, plan={Plan}, remaining={Remaining}",
            activeSub.Id, activeSub.Plan.Name, activeSub.RemainingRequests);

        return new MySubscriptionViewModel
        {
            SubscriptionType = user.SubscriptionType.ToString(),
            AccountTitle = activeSub.Plan.Name,
            TokenLimit = activeSub.Plan.RequestLimit,
            UsedTokens = activeSub.Plan.RequestLimit - activeSub.RemainingRequests,
            QuizUnlocked = activeSub.Plan.AllowQuiz,
            NextRefreshTime = activeSub.RequestWindowStart.AddMinutes(activeSub.Plan.RefreshIntervalMinutes)
        };
    }

    public async Task<bool> IsQuizUnlockedAsync(string userId)
    {
        return await _accessService.CheckCanUseQuizAsync(userId);
    }
}
