using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EduChatbot.Web.Hubs;

[Authorize]
public class AdminHub : Hub<IAdminHubClient>
{
}
