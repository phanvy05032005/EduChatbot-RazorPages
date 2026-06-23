using System;
using System.Threading.Tasks;
using EduChatbot.Data;
using EduChatbot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EduChatbot.Business.Services;

public static class SubscriptionSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Seed Basic plan
        var hasBasic = await context.SubscriptionPlans.AnyAsync(p => p.Name == "Basic");
        if (!hasBasic)
        {
            context.SubscriptionPlans.Add(new SubscriptionPlan
            {
                Name = "Basic",
                Price = 0m,
                DurationDays = null,
                RequestLimit = 20,
                RefreshIntervalMinutes = 1440, // 24 hours
                AllowChat = true,
                AllowQuiz = false,
                TokenLimit = 5000,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Seed Premium plan
        var hasPremium = await context.SubscriptionPlans.AnyAsync(p => p.Name == "Premium");
        if (!hasPremium)
        {
            context.SubscriptionPlans.Add(new SubscriptionPlan
            {
                Name = "Premium",
                Price = 59000m,
                DurationDays = 30,
                RequestLimit = 100,
                RefreshIntervalMinutes = 1440, // 24 hours
                AllowChat = true,
                AllowQuiz = true,
                TokenLimit = 100000,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!hasBasic || !hasPremium)
        {
            await context.SaveChangesAsync();
        }

        // Seed default Course
        var defaultCourse = await context.Courses.FirstOrDefaultAsync(c => c.Code == "PRN222");
        if (defaultCourse == null)
        {
            defaultCourse = new Course
            {
                Code = "PRN222",
                Name = "Đề án .NET",
                Description = "Lập trình ứng dụng với C# và .NET"
            };
            context.Courses.Add(defaultCourse);
            await context.SaveChangesAsync();
        }

        // Link default Course to all documents that do not have a course assigned yet
        var unassignedDocuments = await context.Documents
            .Where(d => d.CourseId == null)
            .ToListAsync();

        if (unassignedDocuments.Any())
        {
            foreach (var doc in unassignedDocuments)
            {
                doc.CourseId = defaultCourse.Id;
            }
            await context.SaveChangesAsync();
        }
    }
}
