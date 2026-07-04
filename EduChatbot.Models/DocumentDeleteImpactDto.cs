namespace EduChatbot.Models;

public class DocumentDeleteImpactDto
{
    public int DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int TotalQuizzes { get; set; }
    public int ActiveQuizzes { get; set; }
    public int TotalAttempts { get; set; }

    public bool HasDependencies => TotalQuizzes > 0;
    public bool HasActiveQuizzes => ActiveQuizzes > 0;
    public bool HasStudentAttempts => TotalAttempts > 0;

    public bool CanHardDelete => TotalAttempts == 0 && ActiveQuizzes == 0;
    public string RecommendedAction { get; set; } = string.Empty;
    public string WarningMessage { get; set; } = string.Empty;
}
