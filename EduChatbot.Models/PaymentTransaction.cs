using EduChatbot.Models.Enums;
using EduChatbot.Models.Identity;

namespace EduChatbot.Models;

public class PaymentTransaction
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public long OrderCode { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";

    public PaymentProvider Provider { get; set; } = PaymentProvider.PAYOS;

    public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;

    public string? CheckoutUrl { get; set; }

    public string? PayOSPaymentLinkId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? SubscriptionId { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = null!;

    public Subscription? Subscription { get; set; }
}
