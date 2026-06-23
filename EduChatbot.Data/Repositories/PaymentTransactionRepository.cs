using EduChatbot.Models;
using EduChatbot.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduChatbot.Data.Repositories;

public class PaymentTransactionRepository : IPaymentTransactionRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentTransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentTransaction?> GetByOrderCodeAsync(long orderCode)
    {
        return await _context.PaymentTransactions
            .Include(pt => pt.User)
            .FirstOrDefaultAsync(pt => pt.OrderCode == orderCode);
    }

    public async Task<PaymentTransaction> AddAsync(PaymentTransaction transaction)
    {
        _context.PaymentTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task UpdateAsync(PaymentTransaction transaction)
    {
        _context.PaymentTransactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasPendingPremiumPaymentAsync(string userId)
    {
        return await _context.PaymentTransactions
            .AnyAsync(pt => pt.UserId == userId && pt.Status == PaymentStatus.PENDING);
    }
}
