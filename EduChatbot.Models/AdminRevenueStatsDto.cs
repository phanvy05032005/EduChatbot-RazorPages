namespace EduChatbot.Models;

public class AdminRevenueStatsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueThisYear { get; set; }
    
    public int TotalPaidOrders { get; set; }
    public int TotalPendingOrders { get; set; }
    public int TotalCancelledOrders { get; set; }
    public int TotalFailedOrders { get; set; }
    
    public int ActivePremiumStudents { get; set; }
    public int ExpiredPremiumSubscriptions { get; set; }
}
