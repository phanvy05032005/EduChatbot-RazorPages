using System.Threading.Tasks;
using EduChatbot.Models;

namespace EduChatbot.Business.Services;

public interface ISubscriptionAccessService
{
    Task EnsureBasicSubscriptionAsync(string userId);
    Task<Subscription?> GetCurrentSubscriptionAsync(string userId);
    Task ReconcileExpiredSubscriptionsAsync(string userId);
    Task CheckCanChatAsync(string userId);
    Task ConsumeChatRequestAsync(string userId);
    Task<bool> CheckCanUseQuizAsync(string userId);
}
