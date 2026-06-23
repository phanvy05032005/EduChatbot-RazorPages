using EduChatbot.Business.Services;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class DashboardModel : PageModel
{
    private readonly IAdminService _adminService;

    public DashboardModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public int TotalStudents { get; private set; }
    public int TotalLecturers { get; private set; }
    public int TotalDocuments { get; private set; }
    public int TotalChatQuestions { get; private set; }
    public List<string> RecentActivities { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var statistics = await _adminService.GetStatisticsAsync();
        TotalStudents = statistics.TotalStudents;
        TotalLecturers = statistics.TotalLecturers;
        TotalDocuments = statistics.TotalDocuments;
        TotalChatQuestions = statistics.TotalQuestionsAsked;
        RecentActivities =
        [
            "Student account created",
            "Lecturer account updated",
            "User role changed",
            "System settings updated"
        ];
    }
}
