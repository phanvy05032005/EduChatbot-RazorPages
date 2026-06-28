using System.Threading.Tasks;
using EduChatbot.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EduChatbot.Web.Services;

public class StudentRealtimeNotifier : IStudentRealtimeNotifier
{
    private readonly IHubContext<EduNotificationHub> _hubContext;

    public StudentRealtimeNotifier(IHubContext<EduNotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyCourseCreatedAsync(StudentCourseCreatedPayload payload)
    {
        await _hubContext.Clients.Group("Students").SendAsync("CourseCreated", payload);
    }

    public async Task NotifyDocumentAvailableAsync(StudentDocumentAvailablePayload payload)
    {
        await _hubContext.Clients.Group("Students").SendAsync("DocumentAvailable", payload);
    }

    public async Task NotifyQuizPublishedAsync(StudentQuizPublishedPayload payload)
    {
        await _hubContext.Clients.Group("Students").SendAsync("QuizPublished", payload);
    }
}
