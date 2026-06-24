using System.Security.Claims;
using EduChatbot.Business.Services;
using EduChatbot.Models;
using EduChatbot.Models.Identity;
using EduChatbot.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EduChatbot.Web.Pages.Documents;

[Authorize(Roles = ApplicationRoles.DocumentManagers)]
[RequestSizeLimit(50 * 1024 * 1024)]
[RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
public class UploadModel : PageModel
{
    private readonly IDocumentService _documentService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStudentRealtimeNotifier _studentRealtimeNotifier;

    public UploadModel(
        IDocumentService documentService,
        IWebHostEnvironment webHostEnvironment,
        UserManager<ApplicationUser> userManager,
        IStudentRealtimeNotifier studentRealtimeNotifier)
    {
        _documentService = documentService;
        _webHostEnvironment = webHostEnvironment;
        _userManager = userManager;
        _studentRealtimeNotifier = studentRealtimeNotifier;
    }

    public List<Course> Courses { get; private set; } = [];

    [BindProperty]
    public IFormFile? DocumentFile { get; set; }

    [BindProperty]
    public int CourseId { get; set; }

    public async Task OnGetAsync()
    {
        await LoadCoursesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (CourseId <= 0)
        {
            ModelState.AddModelError(nameof(CourseId), "Please select a course.");
        }

        if (DocumentFile == null)
        {
            ModelState.AddModelError(nameof(DocumentFile), "Please select a PDF or DOCX file.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync();
            return Page();
        }

        await using var stream = DocumentFile!.OpenReadStream();
        var uploadedBy = await GetCurrentLecturerNameAsync();
        var result = await _documentService.UploadDocumentAsync(
            stream,
            DocumentFile.FileName,
            DocumentFile.ContentType,
            DocumentFile.Length,
            uploadedBy,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            _webHostEnvironment.WebRootPath,
            CourseId);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await LoadCoursesAsync();
            return Page();
        }

        TempData["UploadMessage"] = result.Message;
        TempData["ChunkCount"] = result.ChunkCount;
        TempData["IndexStatus"] = result.Status;

        if (result.Status == DocumentStatuses.Approved && result.CourseId.HasValue)
        {
            await _studentRealtimeNotifier.NotifyDocumentAvailableAsync(new StudentDocumentAvailablePayload
            {
                DocumentId = result.DocumentId,
                CourseId = result.CourseId.Value,
                CourseCode = result.CourseCode ?? string.Empty,
                CourseName = result.CourseName ?? string.Empty,
                FileName = result.FileName ?? string.Empty
            });
        }

        return RedirectToPage("/Documents/Index");
    }

    private async Task LoadCoursesAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var isAdmin = User.IsInRole(ApplicationRoles.Admin);
        Courses = await _documentService.GetAvailableCoursesForUserAsync(userId, isAdmin);
    }

    private async Task<string> GetCurrentLecturerNameAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (!string.IsNullOrWhiteSpace(user?.FullName))
        {
            return user.FullName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(user?.Email))
        {
            return user.Email.Trim();
        }

        return string.IsNullOrWhiteSpace(User.Identity?.Name)
            ? "Lecturer"
            : User.Identity.Name.Trim();
    }
}
