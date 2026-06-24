using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Student
{
    [Authorize(Roles = ApplicationRoles.Student)]
    public class CoursesModel : PageModel
    {
        private readonly IChatService _chatService;

        public CoursesModel(IChatService chatService)
        {
            _chatService = chatService;
        }

        public List<Course> Courses { get; private set; } = [];

        public async Task OnGetAsync()
        {
            Courses = await _chatService.GetCoursesAsync();
        }
    }
}
