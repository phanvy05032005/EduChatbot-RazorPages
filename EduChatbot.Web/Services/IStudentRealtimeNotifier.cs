using System.Threading.Tasks;

namespace EduChatbot.Web.Services;

public class StudentCourseCreatedPayload
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
}

public class StudentDocumentAvailablePayload
{
    public int DocumentId { get; set; }
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

public class StudentQuizPublishedPayload
{
    public int QuizId { get; set; }
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string QuizTitle { get; set; } = string.Empty;
}

public interface IStudentRealtimeNotifier
{
    Task NotifyCourseCreatedAsync(StudentCourseCreatedPayload payload);
    Task NotifyDocumentAvailableAsync(StudentDocumentAvailablePayload payload);
    Task NotifyQuizPublishedAsync(StudentQuizPublishedPayload payload);
}
