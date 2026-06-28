using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduChatbot.Models;

public class QuizQuestion
{
    public int Id { get; set; }

    [Required]
    public int QuizId { get; set; }

    public Quiz? Quiz { get; set; }

    public int QuestionOrder { get; set; }

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public int? SourceChunkId { get; set; }

    public DocumentChunk? SourceChunk { get; set; }

    public List<QuizOption> Options { get; set; } = [];
}
