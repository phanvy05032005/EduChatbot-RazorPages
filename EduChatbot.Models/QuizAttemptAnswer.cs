using System.ComponentModel.DataAnnotations;

namespace EduChatbot.Models;

public class QuizAttemptAnswer
{
    public int Id { get; set; }

    [Required]
    public int QuizAttemptId { get; set; }

    public QuizAttempt? Attempt { get; set; }

    [Required]
    public int QuizQuestionId { get; set; }

    public QuizQuestion? Question { get; set; }

    public int? SelectedOptionId { get; set; }

    public QuizOption? SelectedOption { get; set; }

    public bool? IsCorrect { get; set; }
}
