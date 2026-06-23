namespace EduChatbot.Business.Services;

public class SubscriptionPlanViewModel
{
    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Currency { get; set; } = "VND";

    public int TokenLimit { get; set; }

    public bool QuizUnlocked { get; set; }

    /// <summary>
    /// true nếu user hiện tại đang dùng gói này.
    /// </summary>
    public bool Current { get; set; }

    public int RequestLimit { get; set; }

    public int RefreshIntervalMinutes { get; set; }

    public int? DurationDays { get; set; }

    public int RemainingRequests { get; set; }
}
