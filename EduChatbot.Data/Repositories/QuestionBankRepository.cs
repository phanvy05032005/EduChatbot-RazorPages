using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduChatbot.Models;
using Microsoft.EntityFrameworkCore;

namespace EduChatbot.Data.Repositories;

public class QuestionBankRepository : IQuestionBankRepository
{
    private readonly ApplicationDbContext _context;

    public QuestionBankRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuestionBankItem?> GetByIdAsync(int id)
    {
        return await _context.QuestionBankItems.FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<QuestionBankItem?> GetByIdWithOptionsAsync(int id)
    {
        return await _context.QuestionBankItems
            .Include(q => q.Course)
            .Include(q => q.Document)
            .Include(q => q.SourceChunk)
            .Include(q => q.Options.OrderBy(o => o.OptionOrder))
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<(List<QuestionBankItem> Items, int TotalCount)> SearchQuestionsAsync(QuestionBankFilterDto filter, string? lecturerId, bool isAdmin)
    {
        var query = _context.QuestionBankItems
            .Include(q => q.Course)
            .Include(q => q.Document)
            .AsNoTracking();

        // 1. Ownership / Authorization constraint
        if (!isAdmin && !string.IsNullOrEmpty(lecturerId))
        {
            // Lecturer can only access questions for courses they are assigned to
            var assignedCourseIds = await _context.LecturerCourses
                .Where(lc => lc.LecturerId == lecturerId)
                .Select(lc => lc.CourseId)
                .ToListAsync();

            query = query.Where(q => assignedCourseIds.Contains(q.CourseId));
        }

        // 2. Filters
        if (filter.CourseId.HasValue)
        {
            query = query.Where(q => q.CourseId == filter.CourseId.Value);
        }

        if (filter.DocumentId.HasValue)
        {
            query = query.Where(q => q.DocumentId == filter.DocumentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Difficulty))
        {
            query = query.Where(q => q.Difficulty == filter.Difficulty);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(q => q.Status == filter.Status);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var search = "%" + filter.SearchQuery.Trim() + "%";
            query = query.Where(q => EF.Functions.ILike(q.QuestionText, search) || EF.Functions.ILike(q.Explanation, search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Tag))
        {
            var searchTag = "%" + filter.Tag.Trim() + "%";
            query = query.Where(q => EF.Functions.ILike(q.Tags, searchTag));
        }

        // Total Count
        var totalCount = await query.CountAsync();

        // Paged items
        var items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(QuestionBankItem item)
    {
        await _context.QuestionBankItems.AddAsync(item);
    }

    public Task DeleteAsync(QuestionBankItem item)
    {
        _context.QuestionBankItems.Remove(item);
        return Task.CompletedTask;
    }

    public async Task<List<QuestionBankItem>> GetSelectedItemsAsync(List<int> ids, int courseId)
    {
        return await _context.QuestionBankItems
            .Include(q => q.Options.OrderBy(o => o.OptionOrder))
            .Where(q => ids.Contains(q.Id) && q.CourseId == courseId && q.Status != "Archived")
            .ToListAsync();
    }

    public async Task<bool> IsUsedInQuizzesAsync(int id)
    {
        return await _context.QuizQuestions.AnyAsync(q => q.SourceQuestionBankItemId == id);
    }

    public async Task<bool> ExistsActiveDuplicateAsync(int courseId, string hash)
    {
        return await _context.QuestionBankItems.AnyAsync(q => q.CourseId == courseId && q.Status != "Archived" && q.QuestionTextHash == hash);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
