using EduChatbot.MVC.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduChatbot.MVC.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Dùng Identity service, Controller không thao tác DbContext trực tiếp.
        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return await RedirectByRoleAsync(user);
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
        return View(model);
    }


    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task<IActionResult> RedirectByRoleAsync(ApplicationUser? user)
    {
        if (user != null && await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin))
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        if (user != null && await _userManager.IsInRoleAsync(user, ApplicationRoles.Lecturer))
        {
            return RedirectToAction("Dashboard", "Documents");
        }

        if (user != null && await _userManager.IsInRoleAsync(user, ApplicationRoles.Student))
        {
            return RedirectToAction("Index", "Chat");
        }

        return RedirectToAction("Index", "Home");
    }
}
