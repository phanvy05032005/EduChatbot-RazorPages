using System.Reflection;
using EduChatbot.Business.Services;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class SystemModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public SystemModel(IAdminService adminService, IWebHostEnvironment webHostEnvironment)
    {
        _adminService = adminService;
        _webHostEnvironment = webHostEnvironment;
    }

    public string DatabaseStatus { get; private set; } = "Unknown";
    public string ApplicationStatus { get; private set; } = "Running";
    public string StorageUsage { get; private set; } = "0 MB";
    public string SystemVersion { get; private set; } = "1.0.0";

    public async Task OnGetAsync()
    {
        DatabaseStatus = await _adminService.CanConnectToDatabaseAsync() ? "Connected" : "Unavailable";
        StorageUsage = FormatBytes(GetUploadStorageUsage());
        SystemVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
    }

    private long GetUploadStorageUsage()
    {
        var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
        return Directory.Exists(uploadPath)
            ? Directory.EnumerateFiles(uploadPath, "*", SearchOption.AllDirectories).Sum(file => new FileInfo(file).Length)
            : 0;
    }

    private static string FormatBytes(long bytes)
    {
        return bytes <= 0 ? "0 MB" : $"{bytes / 1024d / 1024d:0.00} MB";
    }
}
