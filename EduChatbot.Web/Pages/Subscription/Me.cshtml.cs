using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Subscription;

[Authorize(Roles = ApplicationRoles.Student)]
public class MeModel : PageModel
{
    private readonly ISubscriptionService _subscriptionService;

    public MeModel(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public MySubscriptionViewModel Subscription { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Subscription = await _subscriptionService.GetMySubscriptionAsync(userId);
        return Page();
    }
}
