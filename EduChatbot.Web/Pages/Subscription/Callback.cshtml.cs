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
            TempData["SubMessage"] = "Không tìm thấy thông tin giao dịch hoặc mã đơn hàng không hợp lệ.";
            TempData["SubMessageType"] = "warning";
            return RedirectToPage("/Subscription/Plans");
        }

        try
        {
            var transaction = await _paymentService.ProcessCallbackAsync(orderCode.Value);
            _logger.LogInformation("Callback synced transaction: orderCode={OrderCode}, status={Status}, previousStatus={Prev}",
                orderCode, transaction.Status, status);

            if (transaction.Status == PaymentStatus.SUCCESS)
            {
                TempData["SubMessage"] = "Thanh toán Premium thành công. Tài khoản của bạn đã được nâng cấp.";
                TempData["SubMessageType"] = "success";
                return RedirectToPage("/Subscription/Me");
            }
            else if (transaction.Status == PaymentStatus.CANCELLED)
            {
                TempData["SubMessage"] = "Thanh toán đã bị hủy. Bạn vẫn đang sử dụng gói Basic.";
                TempData["SubMessageType"] = "warning";
                return RedirectToPage("/Subscription/Plans");
            }
            else if (transaction.Status == PaymentStatus.FAILED)
            {
                TempData["SubMessage"] = "Thanh toán thất bại. Vui lòng thử lại.";
                TempData["SubMessageType"] = "warning";
                return RedirectToPage("/Subscription/Plans");
            }
            else
            {
                TempData["SubMessage"] = "Giao dịch đang chờ xử lý hoặc ở trạng thái không rõ.";
                TempData["SubMessageType"] = "warning";
                return RedirectToPage("/Subscription/Plans");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing callback for orderCode={OrderCode}", orderCode);
            TempData["SubMessage"] = $"Lỗi xác minh thanh toán: {ex.Message}";
            TempData["SubMessageType"] = "warning";
            return RedirectToPage("/Subscription/Plans");
        }
    }
}
