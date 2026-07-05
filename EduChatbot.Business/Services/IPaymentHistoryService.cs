namespace EduChatbot.Business.Services;

public interface IPaymentHistoryService
{
    Task<PagedResult<StudentPaymentHistoryItemViewModel>> GetStudentHistoryAsync(
        string userId,
        int page,
        int pageSize,
        string? sort);

    Task<PagedResult<AdminPaymentHistoryItemViewModel>> GetAdminHistoryAsync(
        PaymentHistoryAdminFilter filter,
        int page,
        int pageSize,
        string? sort);

    Task<PaymentHistoryAdminSummaryViewModel> GetAdminHistorySummaryAsync(PaymentHistoryAdminFilter filter);
}
