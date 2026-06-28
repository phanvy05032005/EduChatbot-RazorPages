using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Lecturer.Quizzes;

[Authorize(Roles = ApplicationRoles.DocumentManagers)]
public class GenerateModel : PageModel
{
    private readonly ILecturerQuizService _lecturerQuizService;
    private readonly IDocumentService _documentService;

    public GenerateModel(ILecturerQuizService lecturerQuizService, IDocumentService documentService)
    {
        _lecturerQuizService = lecturerQuizService;
        _documentService = documentService;
    }

    [BindProperty]
    public LecturerGenerateQuizInput Input { get; set; } = new();

    public List<Course> Courses { get; set; } = [];
    public List<Document> Documents { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        // Fetch courses assigned to this lecturer
        Courses = await _documentService.GetAvailableCoursesForUserAsync(lecturerId, User.IsInRole(ApplicationRoles.Admin));
        
        // Fetch ready documents
        Documents = await _lecturerQuizService.GetReadyDocumentsAsync(lecturerId);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (!ModelState.IsValid)
        {
            Courses = await _documentService.GetAvailableCoursesForUserAsync(lecturerId, User.IsInRole(ApplicationRoles.Admin));
            Documents = await _lecturerQuizService.GetReadyDocumentsAsync(lecturerId);
            return Page();
        }

        try
        {
            var quiz = await _lecturerQuizService.GenerateQuizDraftAsync(Input, lecturerId);
            TempData["SuccessMessage"] = "Quiz generated successfully as Draft.";
            return RedirectToPage("/Lecturer/Quizzes/Review", new { id = quiz.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Failed to generate quiz: " + ex.Message);
            Courses = await _documentService.GetAvailableCoursesForUserAsync(lecturerId, User.IsInRole(ApplicationRoles.Admin));
            Documents = await _lecturerQuizService.GetReadyDocumentsAsync(lecturerId);
            return Page();
        }
    }
}
