using System.ComponentModel.DataAnnotations;

namespace EduChatbot.Models;

public class QuizOption
{
    public int Id { get; set; }

    [Required]
    public int QuizQuestionId { get; set; }

    public QuizQuestion? Question { get; set; }

    public int OptionOrder { get; set; }

    [Required]
    [MaxLength(10)]
    public string Label { get; set; } = string.Empty;

    [Required]
    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
