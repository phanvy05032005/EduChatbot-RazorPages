using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using EduChatbot.Models.Identity;

namespace EduChatbot.Web.Hubs;

[Authorize(Roles = ApplicationRoles.Student)]
public class EduNotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Students");
        await base.OnConnectedAsync();
    }
}
