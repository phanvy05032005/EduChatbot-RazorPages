using System.Collections.Generic;
using System.Threading.Tasks;
using EduChatbot.Models;

namespace EduChatbot.Data.Repositories;

public interface IQuestionBankRepository
{
    Task<QuestionBankItem?> GetByIdAsync(int id);
    Task<QuestionBankItem?> GetByIdWithOptionsAsync(int id);
    Task<(List<QuestionBankItem> Items, int TotalCount)> SearchQuestionsAsync(QuestionBankFilterDto filter, string? lecturerId, bool isAdmin);
    Task AddAsync(QuestionBankItem item);
    Task DeleteAsync(QuestionBankItem item);
    Task<List<QuestionBankItem>> GetSelectedItemsAsync(List<int> ids, int courseId);
    Task<bool> IsUsedInQuizzesAsync(int id);
    Task<bool> ExistsActiveDuplicateAsync(int courseId, string hash);
    Task SaveChangesAsync();
}
