using EduChatbot.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace EduChatbot.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.BASIC;

    public string AccountTitle { get; set; } = "Basic";

    public int TokenLimit { get; set; } = 5000;

    public int UsedTokens { get; set; } = 0;

    /// <summary>
    /// Quiz chỉ mở khi user là PREMIUM. Tính động, không lưu DB.
    /// </summary>
    public bool IsQuizUnlocked => SubscriptionType == SubscriptionType.PREMIUM;
}
