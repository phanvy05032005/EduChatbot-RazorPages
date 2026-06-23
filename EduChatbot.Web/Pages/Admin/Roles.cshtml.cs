using EduChatbot.Web.ViewModels;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class RolesModel : PageModel
{
    public List<AdminRolePermissionViewModel> Roles { get; private set; } = [];

    public void OnGet()
    {
        Roles =
        [
            new() { RoleName = ApplicationRoles.Student, Permissions = ["Login", "Ask Chatbot", "View Own Chat History"] },
            new() { RoleName = ApplicationRoles.Lecturer, Permissions = ["Login", "Upload Documents", "Manage Own Documents", "Run Evaluation"] },
            new() { RoleName = ApplicationRoles.Admin, Permissions = ["Manage Accounts", "Manage Roles", "Manage System", "View Statistics"] }
        ];
    }
}
