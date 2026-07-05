namespace EduChatbot.Business.Services;

public class PaymentHistoryAdminSummaryViewModel
{
    public int TotalTransactions { get; init; }

    public int PaidTransactions { get; init; }

    public decimal TotalRevenue { get; init; }

    public int PendingTransactions { get; init; }

    public int FailedOrCancelledTransactions { get; init; }
}
