using EduChatbot.Business.Services;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.MVC.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class StatisticsModel : PageModel
{
    private readonly IAdminService _adminService;

    public StatisticsModel(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public int TotalStudents { get; private set; }
    public int TotalLecturers { get; private set; }
    public int TotalDocuments { get; private set; }
    public int TotalQuestionsAsked { get; private set; }
    public int MaxValue => new[] { TotalStudents, TotalLecturers, TotalDocuments, TotalQuestionsAsked, 1 }.Max();

    public async Task OnGetAsync()
    {
        var statistics = await _adminService.GetStatisticsAsync();
        TotalStudents = statistics.TotalStudents;
        TotalLecturers = statistics.TotalLecturers;
        TotalDocuments = statistics.TotalDocuments;
        TotalQuestionsAsked = statistics.TotalQuestionsAsked;
    }
}
