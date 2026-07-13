using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Lecturer.QuestionBank;

[Authorize(Roles = ApplicationRoles.AdminAndLecturer)]
public class IndexModel : PageModel
{
    private readonly IQuestionBankService _questionBankService;
    private readonly IDocumentService _documentService;

    public IndexModel(IQuestionBankService questionBankService, IDocumentService documentService)
    {
        _questionBankService = questionBankService;
        _documentService = documentService;
    }

    [BindProperty(SupportsGet = true)]
    public QuestionBankFilterDto Filter { get; set; } = new();

    public PagedResult<QuestionBankItemDto> Questions { get; set; } = null!;
    public List<Course> Courses { get; set; } = [];
    public List<Document> Documents { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole(ApplicationRoles.Admin);

        Courses = await _documentService.GetAvailableCoursesForUserAsync(lecturerId, isAdmin);

        // Fetch documents for assigned courses to populate the filter dropdown
        var docResult = await _documentService.GetDocumentsAsync(null, lecturerId, isAdmin);
        Documents = docResult.Documents.Where(d => d.Status == "Approved").ToList();

        // Perform search/paging query
        Filter.PageSize = 10;
        Questions = await _questionBankService.GetQuestionsAsync(Filter, lecturerId, isAdmin);

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole(ApplicationRoles.Admin);

        try
        {
            var wasArchived = await _questionBankService.DeleteOrArchiveQuestionAsync(id, lecturerId, isAdmin);
            if (wasArchived)
            {
                TempData["SuccessMessage"] = "Question is used in a quiz, and has been successfully Archived instead of deleted.";
            }
            else
            {
                TempData["SuccessMessage"] = "Question deleted successfully.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage();
    }
}
