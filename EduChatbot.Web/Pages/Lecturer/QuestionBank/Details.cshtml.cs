using System.Security.Claims;
using System.Threading.Tasks;
using EduChatbot.Business.Services;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Lecturer.QuestionBank;

[Authorize(Roles = ApplicationRoles.AdminAndLecturer)]
public class DetailsModel : PageModel
{
    private readonly IQuestionBankService _questionBankService;

    public DetailsModel(IQuestionBankService questionBankService)
    {
        _questionBankService = questionBankService;
    }

    public async Task<IActionResult> OnGetDetailsDataAsync(int id)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole(ApplicationRoles.Admin);

        try
        {
            var item = await _questionBankService.GetQuestionByIdAsync(id, lecturerId, isAdmin);
            if (item == null)
            {
                return NotFound();
            }
            return new JsonResult(item);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
