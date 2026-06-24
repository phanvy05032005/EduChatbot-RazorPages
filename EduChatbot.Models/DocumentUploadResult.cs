namespace EduChatbot.Models;

public class DocumentUploadResult
{
    public bool IsSuccess { get; set; }

    public string Message { get; set; } = string.Empty;

    public int DocumentId { get; set; }

    public int ChunkCount { get; set; }

    public string Status { get; set; } = string.Empty;

    public int? CourseId { get; set; }

    public string? CourseCode { get; set; }

    public string? CourseName { get; set; }

    public string? FileName { get; set; }
}
