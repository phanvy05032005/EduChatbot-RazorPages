namespace EduChatbot.Web.Models;

public class AdminRolePermissionViewModel
{
    public string RoleName { get; set; } = string.Empty;

    public List<string> Permissions { get; set; } = [];
}
