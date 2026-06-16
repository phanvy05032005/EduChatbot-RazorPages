using EduChatbot.Business.Services;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduChatbot.MVC.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class LecturersModel : AccountListPageModelBase
{
    public LecturersModel(IAdminService adminService) : base(adminService)
    {
    }

    public override string AccountType => ApplicationRoles.Lecturer;
    public override string Title => "Lecturer Accounts";

    public async Task<IActionResult> OnPostImportAsync(IFormFile? excelFile)
    {
        if (!IsSupportedExcelFile(excelFile))
        {
            TempData["AdminError"] = excelFile == null || excelFile.Length == 0
                ? "Please select an Excel file."
                : "The system only supports .xlsx file format.";
            return RedirectToPage();
        }

        await using var stream = excelFile!.OpenReadStream();
        var result = await AdminService.ImportLecturersFromExcelAsync(stream, true);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage();
    }
}
