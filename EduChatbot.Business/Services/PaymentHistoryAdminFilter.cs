namespace EduChatbot.Business.Services;

public class PaymentHistoryAdminFilter
{
    public string? SearchTerm { get; init; }

    public string? StudentName { get; init; }

    public string? StudentEmail { get; init; }

    public string? OrderCode { get; init; }

    public string? PackageName { get; init; }

    public string? PaymentMethod { get; init; }

    public string? RawPaymentStatus { get; init; }

    public string? DisplayStatus { get; init; }

    public DateTime? CreatedFrom { get; init; }

    public DateTime? CreatedTo { get; init; }

    public DateTime? PaidFrom { get; init; }

    public DateTime? PaidTo { get; init; }

    public DateTime? ExpiredFrom { get; init; }

    public DateTime? ExpiredTo { get; init; }

    public string? PresetRange { get; init; }
}
