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
public class CreateModel : PageModel
{
    private readonly IQuestionBankService _questionBankService;
    private readonly IDocumentService _documentService;

    public CreateModel(IQuestionBankService questionBankService, IDocumentService documentService)
    {
        _questionBankService = questionBankService;
        _documentService = documentService;
    }

    [BindProperty]
    public CreateQuestionBankItemDto Input { get; set; } = new();

    public List<Course> Courses { get; set; } = [];
    public List<Document> Documents { get; set; } = [];

    // Temporary list options for view binding
    [BindProperty]
    public string OptionA { get; set; } = string.Empty;
    [BindProperty]
    public string OptionB { get; set; } = string.Empty;
    [BindProperty]
    public string OptionC { get; set; } = string.Empty;
    [BindProperty]
    public string OptionD { get; set; } = string.Empty;
    [BindProperty]
    public string CorrectOption { get; set; } = "A";

    public async Task<IActionResult> OnGetAsync()
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole(ApplicationRoles.Admin);

        Courses = await _documentService.GetAvailableCoursesForUserAsync(lecturerId, isAdmin);
        var docResult = await _documentService.GetDocumentsAsync(null, lecturerId, isAdmin);
        Documents = docResult.Documents.Where(d => d.Status == "Approved").ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole(ApplicationRoles.Admin);

        // Build the option DTOs manually from fields
        Input.Options = new List<CreateQuestionBankOptionDto>
        {
            new() { OptionText = OptionA, Label = "A", OptionOrder = 1, IsCorrect = CorrectOption == "A" },
            new() { OptionText = OptionB, Label = "B", OptionOrder = 2, IsCorrect = CorrectOption == "B" },
            new() { OptionText = OptionC, Label = "C", OptionOrder = 3, IsCorrect = CorrectOption == "C" },
            new() { OptionText = OptionD, Label = "D", OptionOrder = 4, IsCorrect = CorrectOption == "D" }
        };

        if (string.IsNullOrWhiteSpace(Input.QuestionText))
        {
            ModelState.AddModelError("Input.QuestionText", "Question text is required.");
        }
        if (string.IsNullOrWhiteSpace(OptionA)) ModelState.AddModelError("OptionA", "Option A is required.");
        if (string.IsNullOrWhiteSpace(OptionB)) ModelState.AddModelError("OptionB", "Option B is required.");
        if (string.IsNullOrWhiteSpace(OptionC)) ModelState.AddModelError("OptionC", "Option C is required.");
        if (string.IsNullOrWhiteSpace(OptionD)) ModelState.AddModelError("OptionD", "Option D is required.");

        if (!ModelState.IsValid)
        {
            Courses = await _documentService.GetAvailableCoursesForUserAsync(lecturerId, isAdmin);
            var docResult = await _documentService.GetDocumentsAsync(null, lecturerId, isAdmin);
            Documents = docResult.Documents.Where(d => d.Status == "Approved").ToList();
            return Page();
        }

        try
        {
            await _questionBankService.CreateQuestionAsync(Input, lecturerId);
            TempData["SuccessMessage"] = "Question created successfully.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Courses = await _documentService.GetAvailableCoursesForUserAsync(lecturerId, isAdmin);
            var docResult = await _documentService.GetDocumentsAsync(null, lecturerId, isAdmin);
            Documents = docResult.Documents.Where(d => d.Status == "Approved").ToList();
            return Page();
        }
    }
}
