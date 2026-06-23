namespace EduChatbot.Business.Services;

public class MySubscriptionViewModel
{
    public string SubscriptionType { get; set; } = string.Empty;

    public string AccountTitle { get; set; } = string.Empty;

    public int TokenLimit { get; set; }

    public int UsedTokens { get; set; }

    public int RemainingTokens => TokenLimit - UsedTokens;

    public bool QuizUnlocked { get; set; }

    public DateTime NextRefreshTime { get; set; }
}
