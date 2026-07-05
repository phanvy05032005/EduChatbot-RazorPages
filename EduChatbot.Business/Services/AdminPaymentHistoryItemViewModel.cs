namespace EduChatbot.Business.Services;

public class AdminPaymentHistoryItemViewModel
{
    public string StudentName { get; init; } = string.Empty;

    public string StudentEmail { get; init; } = string.Empty;

    public string PackageName { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Currency { get; init; } = string.Empty;

    public string PaymentMethod { get; init; } = string.Empty;

    public long OrderCode { get; init; }

    public string? ProviderTransactionCode { get; init; }

    public string? ProviderReference { get; init; }

    public string DisplayStatus { get; init; } = string.Empty;

    public string RawPaymentStatus { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime? PaidAt { get; init; }

    public DateTime? ExpiredAt { get; init; }

    public string? StatusReason { get; init; }
}
