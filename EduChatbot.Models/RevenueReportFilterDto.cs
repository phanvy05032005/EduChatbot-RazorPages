using System;
using EduChatbot.Models.Enums;

namespace EduChatbot.Models;

public class RevenueReportFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public PaymentStatus? Status { get; set; }
    public int? SubscriptionPlanId { get; set; }
}
