using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduChatbot.Models;
using Microsoft.EntityFrameworkCore;

namespace EduChatbot.Data.Repositories;

public class QuizRepository : IQuizRepository
{
    private readonly ApplicationDbContext _context;

    public QuizRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz?> GetQuizWithQuestionsAndOptionsAsync(int quizId)
    {
        return await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Document)
            .Include(q => q.Questions.OrderBy(qst => qst.QuestionOrder))
                .ThenInclude(qst => qst.Options.OrderBy(opt => opt.OptionOrder))
            .FirstOrDefaultAsync(q => q.Id == quizId);
    }

    public async Task<Quiz?> GetDraftQuizForLecturerAsync(int quizId, string lecturerId)
    {
        return await _context.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(qst => qst.Options)
            .FirstOrDefaultAsync(q => q.Id == quizId && q.CreatedByLecturerId == lecturerId && q.Status == "Draft");
    }

    public async Task<Quiz?> GetPublishedQuizForStudentAsync(int quizId, string studentId)
    {
        // Currently, all students see all courses/quizzes, so we load the published quiz
        return await _context.Quizzes
            .Include(q => q.Questions.OrderBy(qst => qst.QuestionOrder))
                .ThenInclude(qst => qst.Options.OrderBy(opt => opt.OptionOrder))
            .FirstOrDefaultAsync(q => q.Id == quizId && q.Status == QuizStatuses.Published);
    }

    public async Task<List<Quiz>> GetQuizzesByLecturerAsync(string lecturerId)
    {
        return await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Document)
            .Where(q => q.CreatedByLecturerId == lecturerId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Quiz>> GetAvailablePublishedQuizzesAsync(string studentId)
    {
        // Since students can access all courses currently, we load all published quizzes
        return await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Document)
            .Where(q => q.Status == QuizStatuses.Published)
            .OrderByDescending(q => q.PublishedAt ?? q.CreatedAt)
            .ToListAsync();
    }

    public async Task<QuizAttempt?> GetAttemptForStudentAsync(int attemptId, string studentId)
    {
        return await _context.QuizAttempts
            .Include(a => a.Quiz)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.StudentId == studentId);
    }

    public async Task<QuizAttempt?> GetAttemptWithDetailsAsync(int attemptId, string studentId)
    {
        return await _context.QuizAttempts
            .Include(a => a.Quiz)
                .ThenInclude(q => q!.Questions)
                    .ThenInclude(qst => qst.Options)
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.SelectedOption)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.StudentId == studentId);
    }

    public async Task<int> CountSubmittedAttemptsAsync(int quizId, string studentId)
    {
        return await _context.QuizAttempts
            .CountAsync(a => a.QuizId == quizId && a.StudentId == studentId && a.Status == "Submitted");
    }

    public async Task<QuizAttempt?> GetInProgressAttemptAsync(int quizId, string studentId)
    {
        return await _context.QuizAttempts
            .FirstOrDefaultAsync(a => a.QuizId == quizId && a.StudentId == studentId && a.Status == "InProgress");
    }

    public async Task AddQuizAsync(Quiz quiz)
    {
        await _context.Quizzes.AddAsync(quiz);
    }

    public async Task AddAttemptAsync(QuizAttempt attempt)
    {
        await _context.QuizAttempts.AddAsync(attempt);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
