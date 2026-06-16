using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniExcelLibs;

namespace EduChatbot.MVC.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class CoursesModel : PageModel
{
    private readonly IAdminService _adminService;

    public CoursesModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public List<Course> Courses { get; private set; } = [];
    public List<ApplicationUser> Lecturers { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostCreateCourseAsync(string code, string name, string description)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description))
        {
            TempData["AdminError"] = "Course code, course name, and course description cannot be empty.";
            return RedirectToPage();
        }

        var result = await _adminService.CreateCourseAsync(code, name, description);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteCourseAsync(int id)
    {
        var result = await _adminService.DeleteCourseAsync(id);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAssignLecturerAsync(string lecturerId, int courseId)
    {
        var result = await _adminService.AssignLecturerToCourseAsync(lecturerId, courseId);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveLecturerAsync(string lecturerId, int courseId)
    {
        var result = await _adminService.RemoveLecturerFromCourseAsync(lecturerId, courseId);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostImportCoursesAsync(IFormFile? excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            TempData["AdminError"] = "Please select an Excel file.";
            return RedirectToPage();
        }

        if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["AdminError"] = "The system only supports .xlsx file format.";
            return RedirectToPage();
        }

        await using var stream = excelFile.OpenReadStream();
        var result = await _adminService.ImportCoursesFromExcelAsync(stream);
        TempData[result.IsSuccess ? "AdminMessage" : "AdminError"] = result.Message;
        return RedirectToPage();
    }

    public IActionResult OnGetDownloadTemplate()
    {
        var memoryStream = new MemoryStream();
        var values = new[]
        {
            new { Code = "PRN222", Name = "C# & .NET Cloud", Description = "ASP.NET Core, MVC, Entity Framework Core" },
            new { Code = "SWR302", Name = "Software Requirement", Description = "Software requirements elicitation, analysis, and validation" }
        };
        MiniExcel.SaveAs(memoryStream, values);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CourseImportTemplate.xlsx");
    }

    private async Task LoadAsync()
    {
        Courses = await _adminService.GetCoursesAsync();
        Lecturers = await _adminService.GetLecturersAsync();
    }
}
