using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EduChatbot.Models;

// ==========================================
// LECTURER INPUT DTOs
// ==========================================

public class LecturerGenerateQuizInput
{
    [Required(ErrorMessage = "Course is required")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Document is required")]
    public int DocumentId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Difficulty level is required")]
    [MaxLength(50)]
    public string Difficulty { get; set; } = "Medium";

    [Range(1, 50, ErrorMessage = "Number of questions must be between 1 and 50")]
    public int NumberOfQuestions { get; set; } = 5;

    [Range(1, 180, ErrorMessage = "Time limit must be between 1 and 180 minutes")]
    public int TimeLimitMinutes { get; set; } = 15;

    [Range(1, 10, ErrorMessage = "Max attempts must be between 1 and 10")]
    public int MaxAttempts { get; set; } = 1;

    public string? AdditionalInstruction { get; set; }
}

public class GenerateMoreQuestionsInput
{
    [Required(ErrorMessage = "Number of questions to generate is required.")]
    [Range(1, 10, ErrorMessage = "Can only generate between 1 and 10 questions at a time.")]
    public int AdditionalQuestionCount { get; set; } = 5;

    public string? AdditionalInstruction { get; set; }
}

public class LecturerSaveQuestionInput
{
    public int? QuestionId { get; set; }

    [Required(ErrorMessage = "Question text is required")]
    public string QuestionText { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public List<LecturerSaveOptionInput> Options { get; set; } = [];
}

public class LecturerSaveOptionInput
{
    public int? OptionId { get; set; }

    [Required(ErrorMessage = "Option text is required")]
    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}

// ==========================================
// STUDENT INPUT & TAKE QUIZ DTOs
// ==========================================

public class StudentSubmitQuizInput
{
    public int AttemptId { get; set; }
    public List<StudentSubmitAnswerInput> Answers { get; set; } = [];
}

public class StudentSubmitAnswerInput
{
    public int QuestionId { get; set; }
    public int? SelectedOptionId { get; set; }
}

public class StudentTakeQuizViewModel
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int TimeLimitMinutes { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime DeadlineAt { get; set; }
    public List<TakeQuestionDto> Questions { get; set; } = [];
}

public class TakeQuestionDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int QuestionOrder { get; set; }
    public List<TakeOptionDto> Options { get; set; } = [];
}

public class TakeOptionDto
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int OptionOrder { get; set; }
    public string Label { get; set; } = string.Empty;
}

// ==========================================
// STUDENT RESULT & HISTORY DTOs
// ==========================================

public class StudentQuizResultViewModel
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Score { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<ResultQuestionDto> Questions { get; set; } = [];
}

public class ResultQuestionDto
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int QuestionOrder { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int? SelectedOptionId { get; set; }
    public int CorrectOptionId { get; set; }
    public List<ResultOptionDto> Options { get; set; } = [];
}

public class ResultOptionDto
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public int OptionOrder { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class StudentQuizHistoryItemViewModel
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public double? Score { get; set; }
    public int? CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
}

// ==========================================
// QUESTION BANK DTOs
// ==========================================

public class QuestionBankItemDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public int? DocumentId { get; set; }
    public string? DocumentName { get; set; }
    public int? SourceChunkId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string Difficulty { get; set; } = "Medium";
    public string Status { get; set; } = "Draft";
    public string SourceType { get; set; } = "Manual";
    public string QuestionType { get; set; } = "MultipleChoice";
    public string CreatedByLecturerId { get; set; } = string.Empty;
    public string LecturerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Tags { get; set; }
    public List<QuestionBankOptionDto> Options { get; set; } = [];
}

public class QuestionBankOptionDto
{
    public int Id { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int OptionOrder { get; set; }
    public bool IsCorrect { get; set; }
}

public class QuestionBankFilterDto
{
    public int? CourseId { get; set; }
    public int? DocumentId { get; set; }
    public string? Difficulty { get; set; }
    public string? Status { get; set; }
    public string? SearchQuery { get; set; }
    public string? Tag { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CreateQuestionBankItemDto
{
    [Required(ErrorMessage = "Course is required")]
    public int CourseId { get; set; }
    public int? DocumentId { get; set; }
    public int? SourceChunkId { get; set; }
    [Required(ErrorMessage = "Question text is required")]
    public string QuestionText { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string Difficulty { get; set; } = "Medium";
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft";
    [Required]
    [MaxLength(50)]
    public string QuestionType { get; set; } = "MultipleChoice";
    public string? Tags { get; set; }
    public List<CreateQuestionBankOptionDto> Options { get; set; } = [];
}

public class CreateQuestionBankOptionDto
{
    [Required(ErrorMessage = "Option text is required")]
    public string OptionText { get; set; } = string.Empty;
    [Required]
    [MaxLength(10)]
    public string Label { get; set; } = string.Empty;
    public int OptionOrder { get; set; }
    public bool IsCorrect { get; set; }
}

public class UpdateQuestionBankItemDto
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Question text is required")]
    public string QuestionText { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string Difficulty { get; set; } = "Medium";
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Draft";
    [Required]
    [MaxLength(50)]
    public string QuestionType { get; set; } = "MultipleChoice";
    public string? Tags { get; set; }
    public List<UpdateQuestionBankOptionDto> Options { get; set; } = [];
}

public class UpdateQuestionBankOptionDto
{
    public int? Id { get; set; }
    [Required(ErrorMessage = "Option text is required")]
    public string OptionText { get; set; } = string.Empty;
    [Required]
    [MaxLength(10)]
    public string Label { get; set; } = string.Empty;
    public int OptionOrder { get; set; }
    public bool IsCorrect { get; set; }
}

public class CreateQuizFromBankDto
{
    [Required(ErrorMessage = "Course is required")]
    public int CourseId { get; set; }
    [Required(ErrorMessage = "Quiz title is required")]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string Difficulty { get; set; } = "Medium";
    [Range(1, 180, ErrorMessage = "Time limit must be between 1 and 180 minutes")]
    public int TimeLimitMinutes { get; set; } = 15;
    [Range(1, 10, ErrorMessage = "Max attempts must be between 1 and 10")]
    public int MaxAttempts { get; set; } = 1;
    public string? AdditionalInstruction { get; set; }
    public List<int> SelectedQuestionIds { get; set; } = [];
}

public class CreateQuizFromBankResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? QuizId { get; set; }
}

