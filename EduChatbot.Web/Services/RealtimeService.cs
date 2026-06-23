using System.Threading.Tasks;
using EduChatbot.Business.Services;
using EduChatbot.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EduChatbot.Web.Services;

public class RealtimeService : IRealtimeService
{
    private readonly IHubContext<AdminHub, IAdminHubClient> _hubContext;

    public RealtimeService(IHubContext<AdminHub, IAdminHubClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyAccountChangeAsync(string action, string role)
    {
        await _hubContext.Clients.All.ReceiveAccountChange(action, role);
    }

    public async Task NotifyCourseChangeAsync(string action, string courseCode)
    {
        await _hubContext.Clients.All.ReceiveCourseChange(action, courseCode);
    }

    public async Task NotifyMaterialChangeAsync(string action, string lecturerId, string lecturerName, string fileName)
    {
        await _hubContext.Clients.All.ReceiveMaterialChange(action, lecturerId, lecturerName, fileName);
    }
}
