using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Enums;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace EduChatbot.Web.Pages.Subscription;

[Authorize(Roles = ApplicationRoles.Student)]
public class CallbackModel : PageModel
{
    private readonly IPayOSPaymentService _paymentService;
    private readonly ILogger<CallbackModel> _logger;

    public CallbackModel(
        IPayOSPaymentService paymentService,
        ILogger<CallbackModel> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public bool IsSuccess { get; private set; }
    public bool IsProcessed { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string MessageKey { get; private set; } = string.Empty;
    public string DefaultMessage { get; private set; } = string.Empty;
    public string? ErrorDetail { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        [FromQuery] long? orderCode,
        [FromQuery] string? status,
        [FromQuery] bool? cancel,
        [FromQuery] string? code,
        [FromQuery] string? id)
    {
        _logger.LogInformation("Subscription callback invoked. Query: orderCode={OrderCode}, status={Status}, cancel={Cancel}, code={Code}, id={Id}",
            orderCode, status, cancel, code, id);

        if (orderCode == null || orderCode <= 0)
        {
            _logger.LogWarning("Callback invoked without valid orderCode.");
            IsProcessed = true;
            IsSuccess = false;
            MessageKey = "subscription.callback.errorDesc";
            DefaultMessage = "Invalid order information or error verifying payment.";
            Message = "Không tìm thấy thông tin giao dịch hoặc mã đơn hàng không hợp lệ.";
            return Page();
        }

        try
        {
            var transaction = await _paymentService.ProcessCallbackAsync(orderCode.Value);
            _logger.LogInformation("Callback synced transaction: orderCode={OrderCode}, status={Status}, previousStatus={Prev}",
                orderCode, transaction.Status, status);

            IsProcessed = true;
            if (transaction.Status == PaymentStatus.SUCCESS)
            {
                IsSuccess = true;
                MessageKey = "subscription.callback.successDesc";
                DefaultMessage = "Your account has been upgraded to Premium successfully.";
                Message = "Thanh toán Premium thành công. Tài khoản của bạn đã được nâng cấp.";
            }
            else if (transaction.Status == PaymentStatus.CANCELLED)
            {
                IsSuccess = false;
                MessageKey = "subscription.callback.cancelledDesc";
                DefaultMessage = "The payment was cancelled. You are still on the Basic plan.";
                Message = "Thanh toán đã bị hủy. Bạn vẫn đang sử dụng gói Basic.";
            }
            else if (transaction.Status == PaymentStatus.FAILED)
            {
                IsSuccess = false;
                MessageKey = "subscription.callback.failedDesc";
                DefaultMessage = "The payment failed. Please try again.";
                Message = "Thanh toán thất bại. Vui lòng thử lại.";
            }
            else
            {
                IsSuccess = false;
                MessageKey = "subscription.callback.errorDesc";
                DefaultMessage = "Transaction is pending or in an unknown state.";
                Message = "Giao dịch đang chờ xử lý hoặc ở trạng thái không rõ.";
            }
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing callback for orderCode={OrderCode}", orderCode);
            IsProcessed = true;
            IsSuccess = false;
            MessageKey = "subscription.callback.errorDesc";
            DefaultMessage = $"Payment verification error: {ex.Message}";
            Message = $"Lỗi xác minh thanh toán: {ex.Message}";
            ErrorDetail = ex.Message;
            return Page();
        }
    }
}
