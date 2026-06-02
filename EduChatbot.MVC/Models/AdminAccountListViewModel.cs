namespace EduChatbot.MVC.Models;

public class AdminAccountListViewModel
{
    public string Title { get; set; } = string.Empty;

    public string AccountType { get; set; } = string.Empty;

    public string SearchTerm { get; set; } = string.Empty;

    public List<AdminAccountRowViewModel> Accounts { get; set; } = [];
}
