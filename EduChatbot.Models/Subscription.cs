using System;
using EduChatbot.Models.Enums;
using EduChatbot.Models.Identity;

namespace EduChatbot.Models;

public class Subscription
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public int SubscriptionPlanId { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.PENDING;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int RemainingRequests { get; set; }

    public DateTime RequestWindowStart { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = null!;

    public SubscriptionPlan Plan { get; set; } = null!;
}
