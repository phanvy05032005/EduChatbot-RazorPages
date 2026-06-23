using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Subscription;

[Authorize(Roles = ApplicationRoles.Student)]
public class PlansModel : PageModel
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IPayOSPaymentService _paymentService;

    public PlansModel(
        ISubscriptionService subscriptionService,
        IPayOSPaymentService paymentService)
    {
        _subscriptionService = subscriptionService;
        _paymentService = paymentService;
    }

    public List<SubscriptionPlanViewModel> Plans { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Plans = await _subscriptionService.GetPlansAsync(userId);
        return Page();
    }

    public async Task<IActionResult> OnPostCreatePremiumPaymentAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        try
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var returnUrl = $"{baseUrl}/Subscription/Callback";
            var cancelUrl = $"{baseUrl}/Subscription/Callback";

            var transaction = await _paymentService.CreatePremiumPaymentAsync(
                userId, returnUrl, cancelUrl);

            return Redirect(transaction.CheckoutUrl!);
        }
        catch (InvalidOperationException ex)
        {
            TempData["SubMessage"] = ex.Message;
            TempData["SubMessageType"] = "warning";
            return RedirectToPage();
        }
    }
}
