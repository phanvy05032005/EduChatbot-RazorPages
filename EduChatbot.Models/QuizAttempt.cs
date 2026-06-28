using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EduChatbot.Models.Identity;

namespace EduChatbot.Models;

public class QuizAttempt
{
    public int Id { get; set; }

    [Required]
    public int QuizId { get; set; }

    public Quiz? Quiz { get; set; }

    [Required]
    [MaxLength(450)]
    public string StudentId { get; set; } = string.Empty;

    public ApplicationUser? Student { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "InProgress"; // InProgress, Submitted

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedAt { get; set; }

    public int TotalQuestions { get; set; }

    public int? CorrectCount { get; set; }

    public double? Score { get; set; }

    public List<QuizAttemptAnswer> Answers { get; set; } = [];
}
