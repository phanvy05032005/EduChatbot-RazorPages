using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Admin;

public abstract class AccountListPageModelBase : PageModel
{
    protected readonly IAdminService AdminService;

    protected AccountListPageModelBase(IAdminService adminService)
    {
        AdminService = adminService;
    }

    public abstract string AccountType { get; }

    public abstract string Title { get; }

    public string SearchTerm { get; private set; } = string.Empty;

    public List<AdminAccountRowViewModel> Accounts { get; private set; } = [];

    public async Task OnGetAsync(string? searchTerm)
    {
        await LoadAccountsAsync(searchTerm);
    }

    public async Task<IActionResult> OnPostLockAsync(string id)
    {
        var result = await AdminService.LockAccountAsync(id);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUnlockAsync(string id)
    {
        var result = await AdminService.UnlockAccountAsync(id);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await AdminService.DeleteAccountAsync(id, currentUserId);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage();
    }

    protected async Task LoadAccountsAsync(string? searchTerm)
    {
        SearchTerm = searchTerm?.Trim() ?? string.Empty;
        var accounts = await AdminService.GetAccountsByRoleAsync(AccountType, searchTerm);
        Accounts = accounts
            .Select(account => new AdminAccountRowViewModel
            {
                Id = account.Id,
                FullName = account.FullName,
                Email = account.Email,
                Department = account.Department,
                Status = account.Status
            })
            .ToList();
    }

    protected static bool IsSupportedExcelFile(IFormFile? excelFile)
    {
        return excelFile != null
            && excelFile.Length > 0
            && Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase);
    }
}
