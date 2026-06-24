using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToPage("/Account/Login");
        }

        if (User.IsInRole("Admin"))
        {
            return RedirectToPage("/Admin/Dashboard");
        }

        if (User.IsInRole("Lecturer"))
        {
            return RedirectToPage("/Documents/Dashboard");
        }

        if (User.IsInRole("Student"))
        {
            return RedirectToPage("/Student/Index");
        }

        // Fallback: authenticated but no recognized role
        return RedirectToPage("/Account/Login");
    }
}
