namespace EduChatbot.MVC.Models;

public class AdminSystemStatusViewModel
{
    public string DatabaseStatus { get; set; } = "Unknown";

    public string ApplicationStatus { get; set; } = "Running";

    public string StorageUsage { get; set; } = "0 MB";

    public string SystemVersion { get; set; } = "1.0.0";
}
