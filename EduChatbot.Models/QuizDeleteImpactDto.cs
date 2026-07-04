namespace EduChatbot.Models;

public class QuizDeleteImpactDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalAttempts { get; set; }
    public bool CanHardDelete { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public string WarningMessage { get; set; } = string.Empty;
}
