using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models.Identity;
using EduChatbot.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class UsersModel : PageModel
{
    private readonly IAdminService _adminService;

    public UsersModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public string Title => "User Accounts";

    [BindProperty(SupportsGet = true)]
    public string ActiveTab { get; set; } = "students";

    [BindProperty(SupportsGet = true)]
    public string? StudentSearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? LecturerSearchTerm { get; set; }

    public List<AdminAccountRowViewModel> Students { get; private set; } = [];
    public List<AdminAccountRowViewModel> Lecturers { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostLockAsync(string id, string tab)
    {
        var result = await _adminService.LockAccountAsync(id);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage(new { tab, studentSearchTerm = StudentSearchTerm, lecturerSearchTerm = LecturerSearchTerm });
    }

    public async Task<IActionResult> OnPostUnlockAsync(string id, string tab)
    {
        var result = await _adminService.UnlockAccountAsync(id);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage(new { tab, studentSearchTerm = StudentSearchTerm, lecturerSearchTerm = LecturerSearchTerm });
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id, string tab)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _adminService.DeleteAccountAsync(id, currentUserId);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage(new { tab, studentSearchTerm = StudentSearchTerm, lecturerSearchTerm = LecturerSearchTerm });
    }

    public async Task<IActionResult> OnPostImportStudentsAsync(IFormFile? excelFile)
    {
        if (!IsSupportedExcelFile(excelFile))
        {
            TempData["AdminError"] = excelFile == null || excelFile.Length == 0
                ? "Please select an Excel file."
                : "The system only supports .xlsx file format.";
            return RedirectToPage(new { tab = "students" });
        }

        await using var stream = excelFile!.OpenReadStream();
        var result = await _adminService.ImportStudentsFromExcelAsync(stream, true);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage(new { tab = "students" });
    }

    public async Task<IActionResult> OnPostImportLecturersAsync(IFormFile? excelFile)
    {
        if (!IsSupportedExcelFile(excelFile))
        {
            TempData["AdminError"] = excelFile == null || excelFile.Length == 0
                ? "Please select an Excel file."
                : "The system only supports .xlsx file format.";
            return RedirectToPage(new { tab = "lecturers" });
        }

        await using var stream = excelFile!.OpenReadStream();
        var result = await _adminService.ImportLecturersFromExcelAsync(stream, true);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage(new { tab = "lecturers" });
    }

    private async Task LoadDataAsync()
    {
        // Load Students
        var studentAccounts = await _adminService.GetAccountsByRoleAsync(ApplicationRoles.Student, StudentSearchTerm);
        Students = studentAccounts.Select(a => new AdminAccountRowViewModel
        {
            Id = a.Id,
            FullName = a.FullName,
            Email = a.Email,
            Department = a.Department,
            Status = a.Status
        }).ToList();

        // Load Lecturers
        var lecturerAccounts = await _adminService.GetAccountsByRoleAsync(ApplicationRoles.Lecturer, LecturerSearchTerm);
        Lecturers = lecturerAccounts.Select(a => new AdminAccountRowViewModel
        {
            Id = a.Id,
            FullName = a.FullName,
            Email = a.Email,
            Department = a.Department,
            Status = a.Status
        }).ToList();
    }

    private static bool IsSupportedExcelFile(IFormFile? file)
    {
        return file != null
            && file.Length > 0
            && Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase);
    }
}
