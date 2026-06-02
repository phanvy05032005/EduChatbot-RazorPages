namespace EduChatbot.Business.Services;

public interface IAdminService
{
    Task<AdminStatisticsInfo> GetStatisticsAsync();

    Task<List<AdminAccountInfo>> GetAccountsByRoleAsync(string role, string? searchTerm = null);

    Task<AdminAccountEditInfo?> GetAccountForEditAsync(string id);

    Task<AdminOperationResult> CreateAccountAsync(string fullName, string email, string password, string role);

    Task<AdminOperationResult> UpdateAccountAsync(string id, string fullName, string email);

    Task<AdminOperationResult> LockAccountAsync(string id);

    Task<AdminOperationResult> UnlockAccountAsync(string id);

    Task<AdminOperationResult> DeleteAccountAsync(string id, string currentUserId);

    Task<bool> CanConnectToDatabaseAsync();
}
