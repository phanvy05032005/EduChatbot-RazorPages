using EduChatbot.Data;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduChatbot.Business.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<AdminStatisticsInfo> GetStatisticsAsync()
    {
        return new AdminStatisticsInfo
        {
            TotalStudents = (await _userManager.GetUsersInRoleAsync(ApplicationRoles.Student)).Count,
            TotalLecturers = (await _userManager.GetUsersInRoleAsync(ApplicationRoles.Lecturer)).Count,
            TotalDocuments = await _context.Documents.CountAsync(),
            TotalQuestionsAsked = await _context.ChatMessages.CountAsync(message => message.Role == "user")
        };
    }

    public async Task<List<AdminAccountInfo>> GetAccountsByRoleAsync(string role, string? searchTerm = null)
    {
        var users = await _userManager.GetUsersInRoleAsync(role);
        var query = users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var keyword = searchTerm.Trim().ToLowerInvariant();
            query = query.Where(user =>
                user.Id.ToLowerInvariant().Contains(keyword) ||
                user.FullName.ToLowerInvariant().Contains(keyword) ||
                (user.Email ?? string.Empty).ToLowerInvariant().Contains(keyword));
        }

        return query
            .OrderBy(user => user.Email)
            .Select(user => new AdminAccountInfo
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Department = role == ApplicationRoles.Lecturer ? "General Department" : "N/A",
                Status = IsLocked(user) ? "Locked" : "Active"
            })
            .ToList();
    }

    public async Task<AdminAccountEditInfo?> GetAccountForEditAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new AdminAccountEditInfo
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty
        };
    }

    public async Task<AdminOperationResult> CreateAccountAsync(string fullName, string email, string password, string role)
    {
        if (!IsManageableRole(role))
        {
            return Failure("Invalid account role.");
        }

        var user = new ApplicationUser
        {
            UserName = email.Trim(),
            Email = email.Trim(),
            FullName = fullName.Trim(),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Failure(string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Failure(string.Join(" ", roleResult.Errors.Select(error => error.Description)));
        }

        return Success($"{role} account created successfully.");
    }

    public async Task<AdminOperationResult> UpdateAccountAsync(string id, string fullName, string email)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Failure("Account not found.");
        }

        user.FullName = fullName.Trim();
        user.Email = email.Trim();
        user.UserName = email.Trim();

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? Success("Account updated successfully.")
            : Failure(string.Join(" ", result.Errors.Select(error => error.Description)));
    }

    public async Task<AdminOperationResult> LockAccountAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Failure("Account not found.");
        }

        await _userManager.SetLockoutEnabledAsync(user, true);
        var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

        return result.Succeeded
            ? Success("Account locked successfully.")
            : Failure(string.Join(" ", result.Errors.Select(error => error.Description)));
    }

    public async Task<AdminOperationResult> UnlockAccountAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Failure("Account not found.");
        }

        var result = await _userManager.SetLockoutEndDateAsync(user, null);
        return result.Succeeded
            ? Success("Account unlocked successfully.")
            : Failure(string.Join(" ", result.Errors.Select(error => error.Description)));
    }

    public async Task<AdminOperationResult> DeleteAccountAsync(string id, string currentUserId)
    {
        if (id == currentUserId)
        {
            return Failure("You cannot delete your own admin account.");
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return Failure("Account not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains(ApplicationRoles.Admin))
        {
            return Failure("Admin accounts cannot be deleted from this page.");
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded
            ? Success("Account deleted successfully.")
            : Failure(string.Join(" ", result.Errors.Select(error => error.Description)));
    }

    public Task<bool> CanConnectToDatabaseAsync()
    {
        return _context.Database.CanConnectAsync();
    }

    private static bool IsLocked(ApplicationUser user)
    {
        return user.LockoutEnd.HasValue && user.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow;
    }

    private static bool IsManageableRole(string role)
    {
        return role == ApplicationRoles.Student || role == ApplicationRoles.Lecturer;
    }

    private static AdminOperationResult Success(string message)
    {
        return new AdminOperationResult { IsSuccess = true, Message = message };
    }

    private static AdminOperationResult Failure(string message)
    {
        return new AdminOperationResult { IsSuccess = false, Message = message };
    }
}
