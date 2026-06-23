namespace EduChatbot.MVC.Models;

public class AdminStatisticsViewModel
{
    public int TotalStudents { get; set; }

    public int TotalLecturers { get; set; }

    public int TotalDocuments { get; set; }

    public int TotalQuestionsAsked { get; set; }

    public int MaxValue => new[] { TotalStudents, TotalLecturers, TotalDocuments, TotalQuestionsAsked, 1 }.Max();
}
