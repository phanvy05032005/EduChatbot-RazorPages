using System.Collections.Generic;
using System.Threading.Tasks;
using EduChatbot.Models;

namespace EduChatbot.Business.Services;

public interface IStudentQuizService
{
    Task<List<Quiz>> GetAvailableQuizzesAsync(string studentId);
    Task<QuizAttempt> StartQuizAsync(int quizId, string studentId);
    Task<StudentTakeQuizViewModel> GetTakeQuizAsync(int attemptId, string studentId);
    Task<QuizAttempt> SubmitQuizAsync(int attemptId, string studentId, StudentSubmitQuizInput input);
    Task<StudentQuizResultViewModel> GetResultAsync(int attemptId, string studentId);
    Task<List<StudentQuizHistoryItemViewModel>> GetHistoryAsync(string studentId);
    Task<int> GetSubmittedAttemptsCountAsync(int quizId, string studentId);
    Task<QuizAttempt?> GetInProgressAttemptAsync(int quizId, string studentId);
}
