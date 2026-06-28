namespace EduChatbot.Business.Services;

/// <summary>
/// Config binding cho PayOS keys từ appsettings.json section "PayOS".
/// </summary>
public class PayOSOptions
{
    public const string SectionName = "PayOS";

    public string ClientId { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public string ChecksumKey { get; set; } = string.Empty;

    public string WebhookUrl { get; set; } = string.Empty;

    public bool AutoConfirmWebhook { get; set; }
}
