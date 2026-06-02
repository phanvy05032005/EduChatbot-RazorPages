namespace EduChatbot.MVC.Models;

public class AdminDashboardViewModel
{
    public int TotalStudents { get; set; }

    public int TotalLecturers { get; set; }

    public int TotalDocuments { get; set; }

    public int TotalChatQuestions { get; set; }

    public List<string> RecentActivities { get; set; } = [];
}
