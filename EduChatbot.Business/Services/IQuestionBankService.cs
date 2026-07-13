using System.Collections.Generic;
using System.Threading.Tasks;
using EduChatbot.Models;

namespace EduChatbot.Business.Services;

public interface IQuestionBankService
{
    Task<PagedResult<QuestionBankItemDto>> GetQuestionsAsync(QuestionBankFilterDto filter, string lecturerId, bool isAdmin);
    Task<QuestionBankItemDto?> GetQuestionByIdAsync(int id, string lecturerId, bool isAdmin);
    Task<QuestionBankItemDto> CreateQuestionAsync(CreateQuestionBankItemDto dto, string lecturerId);
    Task UpdateQuestionAsync(UpdateQuestionBankItemDto dto, string lecturerId, bool isAdmin);
    Task<bool> DeleteOrArchiveQuestionAsync(int id, string lecturerId, bool isAdmin);
    Task<(int SavedCount, int SkippedCount)> SaveToBankFromQuizQuestionsAsync(List<int> quizQuestionIds, string lecturerId);
    Task<CreateQuizFromBankResultDto> CreateQuizFromBankAsync(CreateQuizFromBankDto dto, string lecturerId);
}
