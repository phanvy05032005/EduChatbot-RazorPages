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
public class EditModel : PageModel
{
    private readonly IQuestionBankService _questionBankService;

    public EditModel(IQuestionBankService questionBankService)
    {
        _questionBankService = questionBankService;
    }

    [BindProperty]
    public UpdateQuestionBankItemDto Input { get; set; } = new();

    public QuestionBankItemDto QuestionDetails { get; set; } = null!;

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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole(ApplicationRoles.Admin);

        try
        {
            var item = await _questionBankService.GetQuestionByIdAsync(id, lecturerId, isAdmin);
            if (item == null)
            {
                return NotFound("Question not found.");
            }

            if (item.Status == "Archived")
            {
                TempData["ErrorMessage"] = "Cannot edit archived questions.";
                return RedirectToPage("./Index");
            }

            QuestionDetails = item;

            Input.Id = item.Id;
            Input.QuestionText = item.QuestionText;
            Input.Explanation = item.Explanation;
            Input.Difficulty = item.Difficulty;
            Input.Status = item.Status;
            Input.Tags = item.Tags;

            // Map options
            if (item.Options.Count == 4)
            {
                OptionA = item.Options[0].OptionText;
                OptionB = item.Options[1].OptionText;
                OptionC = item.Options[2].OptionText;
                OptionD = item.Options[3].OptionText;

                if (item.Options[0].IsCorrect) CorrectOption = "A";
                else if (item.Options[1].IsCorrect) CorrectOption = "B";
                else if (item.Options[2].IsCorrect) CorrectOption = "C";
                else if (item.Options[3].IsCorrect) CorrectOption = "D";
            }

            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole(ApplicationRoles.Admin);

        // Fetch original question details to prefill Option IDs in DTO
        var item = await _questionBankService.GetQuestionByIdAsync(Input.Id, lecturerId, isAdmin);
        if (item == null)
        {
            return NotFound("Question not found.");
        }
        QuestionDetails = item;

        Input.Options = new List<UpdateQuestionBankOptionDto>
        {
            new() { Id = item.Options[0].Id, OptionText = OptionA, Label = "A", OptionOrder = 1, IsCorrect = CorrectOption == "A" },
            new() { Id = item.Options[1].Id, OptionText = OptionB, Label = "B", OptionOrder = 2, IsCorrect = CorrectOption == "B" },
            new() { Id = item.Options[2].Id, OptionText = OptionC, Label = "C", OptionOrder = 3, IsCorrect = CorrectOption == "C" },
            new() { Id = item.Options[3].Id, OptionText = OptionD, Label = "D", OptionOrder = 4, IsCorrect = CorrectOption == "D" }
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
            return Page();
        }

        try
        {
            await _questionBankService.UpdateQuestionAsync(Input, lecturerId, isAdmin);
            TempData["SuccessMessage"] = "Question updated successfully.";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
