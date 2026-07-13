using System.Collections.Generic;
using System.Linq;
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
public class CreateQuizModel : PageModel
{
    private readonly IQuestionBankService _questionBankService;
    private readonly IDocumentService _documentService;

    public CreateQuizModel(IQuestionBankService questionBankService, IDocumentService documentService)
    {
        _questionBankService = questionBankService;
        _documentService = documentService;
    }

    [BindProperty(SupportsGet = true)]
    public List<int> SelectedIds { get; set; } = [];

    [BindProperty]
    public CreateQuizInputModel Input { get; set; } = new();

    public List<Course> Courses { get; set; } = [];
    public List<QuestionBankItemDto> Questions { get; set; } = [];

    public class CreateQuizInputModel
    {
        public string Title { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int TimeLimit { get; set; } = 15;
        public int MaxAttempts { get; set; } = 3;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (SelectedIds == null || !SelectedIds.Any())
        {
            TempData["ErrorMessage"] = "No questions selected to create a quiz.";
            return RedirectToPage("./Index");
        }

        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole(ApplicationRoles.Admin);

        Courses = await _documentService.GetAvailableCoursesForUserAsync(lecturerId, isAdmin);

        // Fetch selected questions details to display them
        var questionsList = new List<QuestionBankItemDto>();
        foreach (var id in SelectedIds)
        {
            var q = await _questionBankService.GetQuestionByIdAsync(id, lecturerId, isAdmin);
            if (q != null)
            {
                questionsList.Add(q);
            }
        }
        Questions = questionsList;

        if (!Questions.Any())
        {
            TempData["ErrorMessage"] = "The selected questions could not be found or you do not have permission.";
            return RedirectToPage("./Index");
        }

        // Try to pre-select course if all questions belong to the same course
        var uniqueCourseIds = Questions.Select(q => q.CourseId).Distinct().ToList();
        if (uniqueCourseIds.Count == 1)
        {
            Input.CourseId = uniqueCourseIds.First();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole(ApplicationRoles.Admin);

        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError("Input.Title", "Quiz Title is required.");
        }

        if (Input.CourseId <= 0)
        {
            ModelState.AddModelError("Input.CourseId", "Please select a Course.");
        }

        if (Input.TimeLimit <= 0)
        {
            ModelState.AddModelError("Input.TimeLimit", "Time limit must be positive.");
        }

        if (Input.MaxAttempts <= 0)
        {
            ModelState.AddModelError("Input.MaxAttempts", "Maximum attempts must be positive.");
        }

        if (!ModelState.IsValid)
        {
            Courses = await _documentService.GetAvailableCoursesForUserAsync(lecturerId, isAdmin);
            var questionsList = new List<QuestionBankItemDto>();
            foreach (var id in SelectedIds)
            {
                var q = await _questionBankService.GetQuestionByIdAsync(id, lecturerId, isAdmin);
                if (q != null)
                {
                    questionsList.Add(q);
                }
            }
            Questions = questionsList;
            return Page();
        }

        var dto = new CreateQuizFromBankDto
        {
            Title = Input.Title,
            CourseId = Input.CourseId,
            TimeLimitMinutes = Input.TimeLimit,
            MaxAttempts = Input.MaxAttempts,
            SelectedQuestionIds = SelectedIds
        };

        try
        {
            var result = await _questionBankService.CreateQuizFromBankAsync(dto, lecturerId);
            TempData["SuccessMessage"] = $"Quiz created successfully. Result: {result.Message}";
            return RedirectToPage("/Lecturer/Quizzes/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Courses = await _documentService.GetAvailableCoursesForUserAsync(lecturerId, isAdmin);
            var questionsList = new List<QuestionBankItemDto>();
            foreach (var id in SelectedIds)
            {
                var q = await _questionBankService.GetQuestionByIdAsync(id, lecturerId, isAdmin);
                if (q != null)
                {
                    questionsList.Add(q);
                }
            }
            Questions = questionsList;
            return Page();
        }
    }
}
