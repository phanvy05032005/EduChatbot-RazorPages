namespace EduChatbot.Business.Services;

public class AdminOperationResult
{
    public bool IsSuccess { get; set; }

    public string Message { get; set; } = string.Empty;

    public int? CourseId { get; set; }

    public string? CourseCode { get; set; }

    public string? CourseName { get; set; }
}
