using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EduChatbot.Models.Identity;

namespace EduChatbot.Web.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class LecturersModel : PageModel
{
    public IActionResult OnGet()
    {
        return RedirectToPage("/Admin/Users", new { tab = "lecturers" });
    }
}
