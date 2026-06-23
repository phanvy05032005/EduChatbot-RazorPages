using EduChatbot.Web.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LoginModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public void OnGet(string? returnUrl = null)
    {
        Input = new LoginViewModel { ReturnUrl = returnUrl };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            Input.Email,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (!string.IsNullOrWhiteSpace(Input.ReturnUrl))
            {
                return LocalRedirect(Input.ReturnUrl);
            }

            return await RedirectByRoleAsync(user);
        }

        ModelState.AddModelError(string.Empty, "Incorrect email or password.");
        return Page();
    }

    private async Task<IActionResult> RedirectByRoleAsync(ApplicationUser? user)
    {
        if (user != null && await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin))
        {
            return RedirectToPage("/Admin/Dashboard");
        }

        if (user != null && await _userManager.IsInRoleAsync(user, ApplicationRoles.Lecturer))
        {
            return RedirectToPage("/Documents/Dashboard");
        }

        if (user != null && await _userManager.IsInRoleAsync(user, ApplicationRoles.Student))
        {
            return RedirectToPage("/Chat/Index");
        }

        return Redirect("/");
    }
}
