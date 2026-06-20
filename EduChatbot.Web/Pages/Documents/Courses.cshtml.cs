using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Documents;

[Authorize(Roles = ApplicationRoles.DocumentManagers)]
public class CoursesModel : PageModel
{
    private readonly IAdminService _adminService;

    public CoursesModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public List<Course> Courses { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Courses = await _adminService.GetCoursesAsync();
    }
}
