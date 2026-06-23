namespace EduChatbot.Business.Services;

public interface ISubscriptionService
{
    Task<List<SubscriptionPlanViewModel>> GetPlansAsync(string userId);

    Task<MySubscriptionViewModel> GetMySubscriptionAsync(string userId);

    Task<bool> IsQuizUnlockedAsync(string userId);
}
