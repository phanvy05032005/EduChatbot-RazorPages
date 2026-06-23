using EduChatbot.Business.Services;
using EduChatbot.Web.ViewModels;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class AccountFormModel : PageModel
{
    private readonly IAdminService _adminService;

    public AccountFormModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [BindProperty]
    public AdminAccountFormViewModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string? id, string? accountType)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            var account = await _adminService.GetAccountForEditAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            Input = new AdminAccountFormViewModel
            {
                Id = account.Id,
                AccountType = account.Role,
                FullName = account.FullName,
                Email = account.Email
            };

            return Page();
        }

        Input = new AdminAccountFormViewModel
        {
            AccountType = string.IsNullOrWhiteSpace(accountType) ? ApplicationRoles.Student : accountType
        };

        // Always load available courses on GET for unified creation dropdown
        Input.AvailableCourses = await _adminService.GetCoursesAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.IsEdit)
        {
            ModelState.Remove($"{nameof(Input)}.{nameof(AdminAccountFormViewModel.Password)}");
            return await UpdateAccountAsync();
        }

        if (string.IsNullOrWhiteSpace(Input.Password))
        {
            ModelState.AddModelError($"{nameof(Input)}.{nameof(AdminAccountFormViewModel.Password)}", "Please enter the password.");
        }

        if (Input.AccountType == ApplicationRoles.Student)
        {
            Input.SelectedCourseIds = [];
        }

        if (!ModelState.IsValid)
        {
            await LoadCoursesIfNeededAsync();
            return Page();
        }

        var result = await _adminService.CreateAccountAsync(
            Input.FullName,
            Input.Email,
            Input.Password!,
            Input.AccountType,
            true,
            Input.SelectedCourseIds);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await LoadCoursesIfNeededAsync();
            return Page();
        }

        TempData["AdminMessage"] = result.Message;
        return RedirectToAccountList(Input.AccountType);
    }

    private async Task<IActionResult> UpdateAccountAsync()
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Input.Id))
        {
            return Page();
        }

        var result = await _adminService.UpdateAccountAsync(Input.Id, Input.FullName, Input.Email);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        TempData["AdminMessage"] = result.Message;
        return RedirectToAccountList(Input.AccountType);
    }

    private async Task LoadCoursesIfNeededAsync()
    {
        if (!Input.IsEdit)
        {
            Input.AvailableCourses = await _adminService.GetCoursesAsync();
        }
    }

    private IActionResult RedirectToAccountList(string role)
    {
        return role == ApplicationRoles.Lecturer
            ? RedirectToPage("/Admin/Lecturers")
            : RedirectToPage("/Admin/Students");
    }
}
