using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
public class LecturerDetailModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAdminService _adminService;
    private readonly IDocumentService _documentService;

    public LecturerDetailModel(
        UserManager<ApplicationUser> userManager,
        IAdminService adminService,
        IDocumentService documentService)
    {
        _userManager = userManager;
        _adminService = adminService;
        _documentService = documentService;
    }

    public ApplicationUser Lecturer { get; set; } = default!;
    public List<Course> AssignedCourses { get; set; } = [];
    public List<Document> Documents { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var isLecturer = await _userManager.IsInRoleAsync(user, ApplicationRoles.Lecturer);
        if (!isLecturer)
        {
            return NotFound();
        }

        Lecturer = user;
        AssignedCourses = await _adminService.GetLecturerCoursesAsync(id);

        var documentResult = await _documentService.GetDocumentsAsync(null, id, false);
        Documents = documentResult.Documents;

        return Page();
    }

    public async Task<IActionResult> OnGetMaterialsPartialAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest();
        }

        var documentResult = await _documentService.GetDocumentsAsync(null, id, false);
        var documents = documentResult.Documents;

        return Partial("_LecturerMaterialsTable", documents);
    }
}
