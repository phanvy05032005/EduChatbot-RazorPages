using System.Collections.Generic;
using System.Threading.Tasks;
using EduChatbot.Models;

namespace EduChatbot.Data.Repositories;

public interface IQuizRepository
{
    Task<Quiz?> GetQuizWithQuestionsAndOptionsAsync(int quizId);
    Task<Quiz?> GetDraftQuizForLecturerAsync(int quizId, string lecturerId);
    Task<Quiz?> GetPublishedQuizForStudentAsync(int quizId, string studentId);
    Task<List<Quiz>> GetQuizzesByLecturerAsync(string lecturerId);
    Task<List<Quiz>> GetAvailablePublishedQuizzesAsync(string studentId);
    Task<QuizAttempt?> GetAttemptForStudentAsync(int attemptId, string studentId);
    Task<QuizAttempt?> GetAttemptWithDetailsAsync(int attemptId, string studentId);
    Task<int> CountSubmittedAttemptsAsync(int quizId, string studentId);
    Task<QuizAttempt?> GetInProgressAttemptAsync(int quizId, string studentId);
    Task AddQuizAsync(Quiz quiz);
    Task AddAttemptAsync(QuizAttempt attempt);
    Task SaveChangesAsync();
}
