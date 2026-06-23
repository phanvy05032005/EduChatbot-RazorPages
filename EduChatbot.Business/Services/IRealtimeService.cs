using System.Threading.Tasks;

namespace EduChatbot.Business.Services;

public interface IRealtimeService
{
    Task NotifyAccountChangeAsync(string action, string role);
    Task NotifyCourseChangeAsync(string action, string courseCode);
    Task NotifyMaterialChangeAsync(string action, string lecturerId, string lecturerName, string fileName);
}
