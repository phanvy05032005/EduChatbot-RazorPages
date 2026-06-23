using System;

namespace EduChatbot.Models;

public class SubscriptionPlan
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int? DurationDays { get; set; }

    public int RequestLimit { get; set; }

    public int RefreshIntervalMinutes { get; set; }

    public bool AllowChat { get; set; }

    public bool AllowQuiz { get; set; }

    public int TokenLimit { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
