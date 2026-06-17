using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EduChatbot.Data.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { ApplicationRoles.Student, ApplicationRoles.Lecturer, ApplicationRoles.Admin })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await SeedUserAsync(
            userManager,
            "admin@educhatbot.local",
            "Admin@123456",
            "System Admin",
            ApplicationRoles.Admin);

        await SeedUserAsync(
            userManager,
            "student@educhatbot.local",
            "Student@123456",
            "Test Student",
            ApplicationRoles.Student);

        await SeedUserAsync(
            userManager,
            "lecturer@educhatbot.local",
            "Lecturer@123456",
            "Test Lecturer",
            ApplicationRoles.Lecturer);
    }

    private static async Task SeedUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName
            };

            await userManager.CreateAsync(user, password);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
