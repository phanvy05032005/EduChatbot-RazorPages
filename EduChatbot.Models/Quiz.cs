using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EduChatbot.Models.Identity;

namespace EduChatbot.Models;

public class Quiz
{
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }

    public Course? Course { get; set; }

    [Required]
    public int DocumentId { get; set; }

    public Document? Document { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Difficulty { get; set; } = string.Empty;

    public string AdditionalInstruction { get; set; } = string.Empty;

    public int NumberOfQuestions { get; set; }

    public int TimeLimitMinutes { get; set; }

    public int MaxAttempts { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft"; // Draft, Published

    [Required]
    [MaxLength(450)]
    public string CreatedByLecturerId { get; set; } = string.Empty;

    public ApplicationUser? CreatedByLecturer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PublishedAt { get; set; }

    public List<QuizQuestion> Questions { get; set; } = [];

    public List<QuizAttempt> Attempts { get; set; } = [];
}

public static class QuizStatuses
{
    public const string Draft = "Draft";
    public const string Published = "Published";
    public const string Archived = "Archived";
}
