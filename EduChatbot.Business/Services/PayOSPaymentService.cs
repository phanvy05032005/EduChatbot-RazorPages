using EduChatbot.Data;
using EduChatbot.Data.Repositories;
using EduChatbot.Models;
using EduChatbot.Models.Enums;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PayOS;
using PayOS.Models.Webhooks;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CreatePaymentLinkRequest = PayOS.Models.V2.PaymentRequests.CreatePaymentLinkRequest;

namespace EduChatbot.Business.Services;

public class PayOSPaymentService : IPayOSPaymentService
{
    private const decimal PremiumPrice = 59000;
    private const string PremiumDescription = "EduChatbot Premium";

    private readonly PayOSClient _payOSClient;
    private readonly IPaymentTransactionRepository _transactionRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ISubscriptionAccessService _accessService;
    private readonly ILogger<PayOSPaymentService> _logger;

    public PayOSPaymentService(
        PayOSClient payOSClient,
        IPaymentTransactionRepository transactionRepo,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ISubscriptionAccessService accessService,
        ILogger<PayOSPaymentService> logger)
    {
        _payOSClient = payOSClient;
        _transactionRepo = transactionRepo;
        _userManager = userManager;
        _context = context;
        _accessService = accessService;
        _logger = logger;
    }

    public async Task<PaymentTransaction> CreatePremiumPaymentAsync(
        string userId, string returnUrl, string cancelUrl)
    {
        // 1. Reconcile expired subscriptions
        await _accessService.ReconcileExpiredSubscriptionsAsync(userId);
        await _accessService.EnsureBasicSubscriptionAsync(userId);

        // Validate user tồn tại
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        // Block if user already has an active Premium plan
        var currentSub = await _accessService.GetCurrentSubscriptionAsync(userId);
        if (currentSub != null && currentSub.Plan.Name.Equals("Premium", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Bạn đang sử dụng gói Premium.");
        }

        // Check for duplicate pending payments
        var pendingTransactions = await _context.PaymentTransactions
            .Where(pt => pt.UserId == userId && pt.Status == PaymentStatus.PENDING)
            .ToListAsync();

        bool hasFreshPending = false;
        var now = DateTime.UtcNow;

        foreach (var pt in pendingTransactions)
        {
            if (now - pt.CreatedAt >= TimeSpan.FromMinutes(15))
            {
                // Mark stale payment as FAILED
                pt.Status = PaymentStatus.FAILED;
                pt.UpdatedAt = now;

                // Mark related pending Premium subscription as CANCELLED
                if (pt.SubscriptionId.HasValue)
                {
                    var sub = await _context.Subscriptions.FindAsync(pt.SubscriptionId.Value);
                    if (sub != null && sub.Status == SubscriptionStatus.PENDING)
                    {
                        sub.Status = SubscriptionStatus.CANCELLED;
                        sub.UpdatedAt = now;
                    }
                }
                else
                {
                    // Fallback: find any pending Premium subscription for the user
                    var pendingPremium = await _context.Subscriptions
                        .Include(s => s.Plan)
                        .FirstOrDefaultAsync(s => s.UserId == userId && s.Plan.Name == "Premium" && s.Status == SubscriptionStatus.PENDING);
                    if (pendingPremium != null)
                    {
                        pendingPremium.Status = SubscriptionStatus.CANCELLED;
                        pendingPremium.UpdatedAt = now;
                    }
                }
            }
            else
            {
                hasFreshPending = true;
            }
        }

        await _context.SaveChangesAsync();

        if (hasFreshPending)
        {
            throw new InvalidOperationException("Bạn đang có một giao dịch Premium chờ xử lý.");
        }

        // Get Premium Plan
        var premiumPlan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Name == "Premium")
            ?? throw new InvalidOperationException("Premium plan is missing from database.");

        // Create PENDING Premium subscription
        var premiumSubscription = new Subscription
        {
            UserId = userId,
            SubscriptionPlanId = premiumPlan.Id,
            Status = SubscriptionStatus.PENDING,
            StartDate = null,
            EndDate = null,
            RemainingRequests = premiumPlan.RequestLimit,
            RequestWindowStart = now,
            CreatedAt = now
        };
        _context.Subscriptions.Add(premiumSubscription);
        await _context.SaveChangesAsync(); // populated ID

        // Tạo orderCode unique dựa trên timestamp
        var orderCode = GenerateOrderCode();

        // Gọi PayOS tạo payment link
        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = (int)PremiumPrice,
            Description = PremiumDescription,
            ReturnUrl = returnUrl,
            CancelUrl = cancelUrl,
            ExpiredAt = (int)DateTimeOffset.UtcNow.AddSeconds(90).ToUnixTimeSeconds()
        };

        var paymentLink = await _payOSClient.PaymentRequests.CreateAsync(paymentRequest);

        // Lưu transaction PENDING trước khi redirect
        var transaction = new PaymentTransaction
        {
            UserId = userId,
            OrderCode = orderCode,
            Amount = PremiumPrice,
            Currency = "VND",
            Provider = PaymentProvider.PAYOS,
            Status = PaymentStatus.PENDING,
            CheckoutUrl = paymentLink.CheckoutUrl,
            PayOSPaymentLinkId = paymentLink.PaymentLinkId,
            SubscriptionId = premiumSubscription.Id,
            CreatedAt = now
        };

        _context.PaymentTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created Premium payment for user {UserId}, orderCode={OrderCode}, checkoutUrl={Url}",
            userId, orderCode, paymentLink.CheckoutUrl);

        return transaction;
    }

    public async Task<PaymentTransaction> ProcessCallbackAsync(long orderCode)
    {
        return (await SyncTransactionStatusAsync(orderCode, throwIfMissing: true))!;
    }

    public async Task<PaymentTransaction?> ProcessWebhookAsync(string webhookPayload)
    {
        if (string.IsNullOrWhiteSpace(webhookPayload))
        {
            throw new InvalidOperationException("Webhook payload trống.");
        }

        Webhook webhook;

        try
        {
            webhook = JsonSerializer.Deserialize<Webhook>(
                webhookPayload,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new InvalidOperationException("Webhook payload không hợp lệ.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Unable to deserialize PayOS webhook payload.");
            throw new InvalidOperationException("Webhook payload không hợp lệ.", ex);
        }

        try
        {
            var verifiedWebhook = await _payOSClient.Webhooks.VerifyAsync(webhook);
            _logger.LogInformation("PayOS webhook verified successfully for orderCode={OrderCode}", verifiedWebhook.OrderCode);
            return await SyncTransactionStatusAsync(verifiedWebhook.OrderCode, throwIfMissing: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to verify PayOS webhook.");
            throw new InvalidOperationException("Không thể xác minh webhook PayOS.", ex);
        }
    }

    private async Task<PaymentTransaction?> SyncTransactionStatusAsync(long orderCode, bool throwIfMissing = true)
    {
        var transaction = await _context.PaymentTransactions
            .Include(pt => pt.User)
            .Include(pt => pt.Subscription)
            .FirstOrDefaultAsync(pt => pt.OrderCode == orderCode);

        if (transaction == null)
        {
            if (throwIfMissing)
            {
                throw new InvalidOperationException("Không tìm thấy giao dịch.");
            }
            _logger.LogWarning("PayOS sync status ignored because orderCode was not found locally. orderCode={OrderCode}", orderCode);
            return null;
        }

        // Idempotent: đã xử lý rồi thì trả luôn
        if (transaction.Status == PaymentStatus.SUCCESS)
        {
            _logger.LogInformation(
                "Callback for orderCode={OrderCode} already processed as SUCCESS.", orderCode);
            return transaction;
        }

        if (transaction.Status == PaymentStatus.CANCELLED || transaction.Status == PaymentStatus.FAILED)
        {
            _logger.LogInformation(
                "Callback for orderCode={OrderCode} already marked as {Status}.", orderCode, transaction.Status);
            return transaction;
        }

        // Gọi PayOS API kiểm tra trạng thái thật
        try
        {
            var paymentInfo = await _payOSClient.PaymentRequests.GetAsync(orderCode);
            var paymentStatus = paymentInfo.Status.ToString();

            _logger.LogInformation(
                "PayOS status for orderCode={OrderCode}: {Status}", orderCode, paymentStatus);

            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.UtcNow;

                if (string.Equals(paymentStatus, "PAID", StringComparison.OrdinalIgnoreCase))
                {
                    // Thanh toán thành công
                    transaction.Status = PaymentStatus.SUCCESS;
                    transaction.PaidAt = now;
                    transaction.UpdatedAt = now;

                    // Upgrade Premium subscription
                    Subscription? premiumSub = transaction.Subscription;
                    if (premiumSub == null && transaction.SubscriptionId.HasValue)
                    {
                        premiumSub = await _context.Subscriptions.FindAsync(transaction.SubscriptionId.Value);
                    }
                    if (premiumSub == null)
                    {
                        // Fallback query
                        premiumSub = await _context.Subscriptions
                            .Include(s => s.Plan)
                            .FirstOrDefaultAsync(s => s.UserId == transaction.UserId && s.Plan.Name == "Premium" && s.Status == SubscriptionStatus.PENDING);
                    }

                    if (premiumSub != null)
                    {
                        var premiumPlan = await _context.SubscriptionPlans
                            .FirstOrDefaultAsync(p => p.Name == "Premium")
                            ?? throw new InvalidOperationException("Premium plan is missing from database.");

                        premiumSub.Status = SubscriptionStatus.ACTIVE;
                        premiumSub.StartDate = now;
                        premiumSub.EndDate = now.AddDays(premiumPlan.DurationDays ?? 30);
                        premiumSub.RemainingRequests = premiumPlan.RequestLimit;
                        premiumSub.RequestWindowStart = now;
                        premiumSub.UpdatedAt = now;
                    }

                    // Suspend active Basic subscription
                    var basicPlan = await _context.SubscriptionPlans
                        .FirstOrDefaultAsync(p => p.Name == "Basic")
                        ?? throw new InvalidOperationException("Basic plan is missing from database.");

                    var basicSub = await _context.Subscriptions
                        .FirstOrDefaultAsync(s => s.UserId == transaction.UserId && s.SubscriptionPlanId == basicPlan.Id && s.Status == SubscriptionStatus.ACTIVE);

                    if (basicSub != null)
                    {
                        basicSub.Status = SubscriptionStatus.SUSPENDED;
                        basicSub.UpdatedAt = now;
                    }

                    // Upgrade user sang PREMIUM (compatibility fields)
                    var user = transaction.User;
                    if (user != null)
                    {
                        user.SubscriptionType = SubscriptionType.PREMIUM;
                        user.AccountTitle = "Premium";
                        user.TokenLimit = 100000;
                        user.UsedTokens = 0;
                        await _userManager.UpdateAsync(user);
                    }

                    _logger.LogInformation("User {UserId} upgraded to PREMIUM successfully.", transaction.UserId);
                }
                else if (string.Equals(paymentStatus, "CANCELLED", StringComparison.OrdinalIgnoreCase))
                {
                    transaction.Status = PaymentStatus.CANCELLED;
                    transaction.UpdatedAt = now;

                    // Mark related Premium subscription as CANCELLED
                    Subscription? premiumSub = transaction.Subscription;
                    if (premiumSub != null)
                    {
                        premiumSub.Status = SubscriptionStatus.CANCELLED;
                        premiumSub.UpdatedAt = now;
                    }

                    // Ensure Basic is ACTIVE
                    var basicPlan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "Basic");
                    if (basicPlan != null)
                    {
                        var basicSub = await _context.Subscriptions
                            .FirstOrDefaultAsync(s => s.UserId == transaction.UserId && s.SubscriptionPlanId == basicPlan.Id);
                        if (basicSub != null && basicSub.Status == SubscriptionStatus.SUSPENDED)
                        {
                            basicSub.Status = SubscriptionStatus.ACTIVE;
                            basicSub.UpdatedAt = now;
                        }
                    }
                }
                else
                {
                    // Nếu PayOS trả EXPIRED hoặc status lạ khác PENDING, ta đánh FAILED
                    if (!string.Equals(paymentStatus, "PENDING", StringComparison.OrdinalIgnoreCase))
                    {
                        transaction.Status = PaymentStatus.FAILED;
                        transaction.UpdatedAt = now;

                        Subscription? premiumSub = transaction.Subscription;
                        if (premiumSub != null)
                        {
                            premiumSub.Status = SubscriptionStatus.CANCELLED;
                            premiumSub.UpdatedAt = now;
                        }

                        // Ensure Basic is ACTIVE
                        var basicPlan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "Basic");
                        if (basicPlan != null)
                        {
                            var basicSub = await _context.Subscriptions
                                .FirstOrDefaultAsync(s => s.UserId == transaction.UserId && s.SubscriptionPlanId == basicPlan.Id);
                            if (basicSub != null && basicSub.Status == SubscriptionStatus.SUSPENDED)
                            {
                                basicSub.Status = SubscriptionStatus.ACTIVE;
                                basicSub.UpdatedAt = now;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                throw new InvalidOperationException("Lỗi cập nhật trạng thái giao dịch trong DB.", ex);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error verifying PayOS payment for orderCode={OrderCode}", orderCode);
            throw new InvalidOperationException(
                "Không thể xác minh thanh toán, vui lòng thử lại.", ex);
        }

        return transaction;
    }

    private static long GenerateOrderCode()
    {
        var ticks = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var random = Random.Shared.Next(1000);
        return (ticks % 10000000000) * 1000 + random;
    }
}
