using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EduChatbot.Models.Identity;

namespace EduChatbot.Models;

public class QuestionBankItem
{
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }

    public Course? Course { get; set; }

    public int? DocumentId { get; set; }

    public Document? Document { get; set; }

    public int? SourceChunkId { get; set; }

    public DocumentChunk? SourceChunk { get; set; }

    [Required]
    public string QuestionText { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string QuestionTextHash { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Difficulty { get; set; } = "Medium";

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft"; // Draft, Approved, Archived

    [Required]
    [MaxLength(50)]
    public string SourceType { get; set; } = "Manual"; // AI, Manual

    [Required]
    [MaxLength(50)]
    public string QuestionType { get; set; } = "MultipleChoice";

    [Required]
    [MaxLength(450)]
    public string CreatedByLecturerId { get; set; } = string.Empty;

    public ApplicationUser? CreatedByLecturer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string Tags { get; set; } = string.Empty;

    public List<QuestionBankOption> Options { get; set; } = [];
}
