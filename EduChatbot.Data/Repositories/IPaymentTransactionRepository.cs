using EduChatbot.Models;

namespace EduChatbot.Data.Repositories;

public interface IPaymentTransactionRepository
{
    Task<PaymentTransaction?> GetByOrderCodeAsync(long orderCode);

    Task<PaymentTransaction> AddAsync(PaymentTransaction transaction);

    Task UpdateAsync(PaymentTransaction transaction);

    Task<bool> HasPendingPremiumPaymentAsync(string userId);
}
