namespace EduChatbot.Business.Services;

public class AdminAccountInfo
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Department { get; set; } = "General";

    public string Status { get; set; } = "Active";
}
