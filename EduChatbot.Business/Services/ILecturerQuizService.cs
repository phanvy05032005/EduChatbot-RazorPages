using System.Collections.Generic;
using System.Threading.Tasks;
using EduChatbot.Models;

namespace EduChatbot.Business.Services;

public interface ILecturerQuizService
{
    Task<List<Quiz>> GetLecturerQuizzesAsync(string lecturerId);
    Task<List<Document>> GetReadyDocumentsAsync(string lecturerId);
    Task<Quiz> GenerateQuizDraftAsync(LecturerGenerateQuizInput input, string lecturerId);
    Task<Quiz?> GetQuizForReviewAsync(int quizId, string lecturerId);
    Task UpdateQuestionAsync(int quizId, string lecturerId, LecturerSaveQuestionInput input);
    Task DeleteQuestionAsync(int quizId, int questionId, string lecturerId);
    Task PublishQuizAsync(int quizId, string lecturerId);
    Task<List<QuizAttempt>> GetQuizAttemptsAsync(int quizId, string lecturerId);
    Task<QuizQuestion> AddQuestionAsync(int quizId, string lecturerId, LecturerSaveQuestionInput input);
    Task<List<QuizQuestion>> GenerateMoreQuestionsAsync(int quizId, string lecturerId, GenerateMoreQuestionsInput input);
    Task ArchiveQuizAsync(int quizId, string lecturerId);
}
