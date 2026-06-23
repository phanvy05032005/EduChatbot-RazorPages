using EduChatbot.Web.ViewModels;
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
                ModelState.AddModelError(string.Empty, MapIdentityErrorToVietnamese(error));
            }

            return Page();
        }

        user.HasChangedPassword = true;
        await _userManager.UpdateAsync(user);

        await _signInManager.RefreshSignInAsync(user);
        TempData["ProfileMessage"] = "Password changed successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangePasswordAjaxAsync(string currentPassword, string newPassword, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            return new JsonResult(new { success = false, message = "Vui lòng điền đầy đủ các thông tin." });
        }

        if (newPassword != confirmPassword)
        {
            return new JsonResult(new { success = false, message = "Mật khẩu mới và mật khẩu xác nhận không khớp." });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return new JsonResult(new { success = false, message = "Không tìm thấy thông tin tài khoản." });
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var errorMessages = result.Errors.Select(e => MapIdentityErrorToVietnamese(e));
            return new JsonResult(new { success = false, message = string.Join(" ", errorMessages) });
        }

        user.HasChangedPassword = true;
        await _userManager.UpdateAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        return new JsonResult(new { success = true, message = "Thay đổi mật khẩu thành công!" });
    }

    private string MapIdentityErrorToVietnamese(IdentityError error)
    {
        return error.Code switch
        {
            "PasswordMismatch" => "Mật khẩu hiện tại không chính xác.",
            "PasswordTooShort" => "Mật khẩu phải có ít nhất 6 ký tự.",
            "PasswordRequiresNonAlphanumeric" => "Mật khẩu phải chứa ít nhất một ký tự đặc biệt (ví dụ: @, #, $, ...).",
            "PasswordRequiresDigit" => "Mật khẩu phải chứa ít nhất một chữ số ('0'-'9').",
            "PasswordRequiresLower" => "Mật khẩu phải chứa ít nhất một chữ cái thường ('a'-'z').",
            "PasswordRequiresUpper" => "Mật khẩu phải chứa ít nhất một chữ cái hoa ('A'-'Z').",
            "PasswordRequiresUniqueChars" => "Mật khẩu phải chứa nhiều ký tự khác nhau hơn.",
            _ => error.Description
        };
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
