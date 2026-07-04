namespace EduChatbot.Models;

public class DocumentDeleteResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ExecutedAction { get; set; } = string.Empty;
}
