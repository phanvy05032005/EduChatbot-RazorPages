using EduChatbot.Web.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Account;

[Authorize(Roles = ApplicationRoles.Student + "," + ApplicationRoles.Lecturer)]
public class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ProfileModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public AccountProfileViewModel ProfileInput { get; set; } = new();

    [BindProperty]
    public AccountChangePasswordViewModel PasswordInput { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Account/Login");
        }

        LoadProfile(user);
        return Page();
    }

    public async Task<IActionResult> OnPostProfileAsync()
    {
        ClearPasswordModelState();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Account/Login");
        }

        ProfileInput.Email = user.Email ?? string.Empty;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        user.FullName = ProfileInput.FullName.Trim();
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["ProfileMessage"] = "Profile updated successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        ClearProfileModelState();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToPage("/Account/Login");
        }

        LoadProfile(user);
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            PasswordInput.CurrentPassword,
            PasswordInput.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["ProfileMessage"] = "Password changed successfully.";
        return RedirectToPage();
    }

    private void LoadProfile(ApplicationUser user)
    {
        ProfileInput = new AccountProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty
        };
    }

    private void ClearPasswordModelState()
    {
        ModelState.Remove($"{nameof(PasswordInput)}.{nameof(AccountChangePasswordViewModel.CurrentPassword)}");
        ModelState.Remove($"{nameof(PasswordInput)}.{nameof(AccountChangePasswordViewModel.NewPassword)}");
        ModelState.Remove($"{nameof(PasswordInput)}.{nameof(AccountChangePasswordViewModel.ConfirmPassword)}");
    }

    private void ClearProfileModelState()
    {
        ModelState.Remove($"{nameof(ProfileInput)}.{nameof(AccountProfileViewModel.FullName)}");
        ModelState.Remove($"{nameof(ProfileInput)}.{nameof(AccountProfileViewModel.Email)}");
    }
}
