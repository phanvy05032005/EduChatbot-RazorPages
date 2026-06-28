using EduChatbot.Models;

namespace EduChatbot.Business.Services;

public interface IPayOSPaymentService
{
    /// <summary>
    /// Tạo payment link PayOS cho gói Premium. Lưu transaction PENDING trước khi trả về.
    /// </summary>
    Task<PaymentTransaction> CreatePremiumPaymentAsync(string userId, string returnUrl, string cancelUrl);

    /// <summary>
    /// Xử lý callback từ PayOS: verify trạng thái giao dịch thật, update transaction + user.
    /// Idempotent: gọi nhiều lần không lỗi.
    /// </summary>
    Task<PaymentTransaction> ProcessCallbackAsync(long orderCode);

    /// <summary>
    /// Xử lý webhook JSON từ PayOS, verify chữ ký rồi đồng bộ transaction thật từ PayOS.
    /// </summary>
    Task<PaymentTransaction?> ProcessWebhookAsync(string webhookPayload);
}
