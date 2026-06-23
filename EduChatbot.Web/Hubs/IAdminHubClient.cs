using System.Threading.Tasks;

namespace EduChatbot.Web.Hubs;

public interface IAdminHubClient
{
    Task ReceiveAccountChange(string action, string role);
    Task ReceiveCourseChange(string action, string courseCode);
    Task ReceiveMaterialChange(string action, string lecturerId, string lecturerName, string fileName);
}
