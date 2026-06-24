using System;

namespace EduChatbot.Models;

public class ChatConversationSummary
{
    public int Id { get; set; }

    public string DisplayTitle { get; set; } = string.Empty;

    public string? DisplayPreview { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public string? CourseCode { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int MessageCount { get; set; }
}
