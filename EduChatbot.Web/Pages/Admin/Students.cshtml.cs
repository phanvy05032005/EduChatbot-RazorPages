using EduChatbot.Business.Services;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduChatbot.Web.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class StudentsModel : AccountListPageModelBase
{
    public StudentsModel(IAdminService adminService) : base(adminService)
    {
    }

    public override string AccountType => ApplicationRoles.Student;
    public override string Title => "Student Accounts";

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
        var result = await AdminService.ImportStudentsFromExcelAsync(stream, true);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage();
    }
}
