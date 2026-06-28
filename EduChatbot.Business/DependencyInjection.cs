using EduChatbot.Business.Services;
using EduChatbot.Data;
using EduChatbot.Data.Identity;
using EduChatbot.Data.Repositories;
using EduChatbot.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayOS;
using Pgvector.EntityFrameworkCore;

namespace EduChatbot.Business;

public static class DependencyInjection
{
    public static IServiceCollection AddEduChatbotApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.UseVector())
            // NOTE: Dev/test: cho phép chạy migration update dù model snapshot còn lệch naming convention ở các bảng khác.
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
        });

        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddSingleton<IDocumentUploadRules, DocumentUploadRules>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddHttpClient<ILecturerQuizService, LecturerQuizService>();
        services.AddScoped<IStudentQuizService, StudentQuizService>();

        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IChatService, ChatService>();
        services.Configure<OpenRouterSettings>(configuration.GetSection("OpenRouter"));
        services.Configure<EmbeddingSettings>(configuration.GetSection("Embedding"));
        services.Configure<ChatSettings>(configuration.GetSection("Chat"));
        services.AddHttpClient<IEmbeddingService, OpenRouterEmbeddingService>();
        services.AddHttpClient<IChatService, ChatService>();
        services.AddScoped<IEmailService, EmailService>();

        // Email Queue (DB) + Worker
        services.AddScoped<IEmailQueueRepository, EmailQueueRepository>();
        services.AddScoped<IEmailQueueService, EmailQueueService>();
        services.AddHostedService<EmailQueueWorker>();

        // PayOS + Subscription + Payment
        services.Configure<EduChatbot.Business.Services.PayOSOptions>(
            configuration.GetSection(EduChatbot.Business.Services.PayOSOptions.SectionName));

        services.AddSingleton<PayOSClient>(sp =>
        {
            var payosSection = configuration.GetSection(EduChatbot.Business.Services.PayOSOptions.SectionName);
            var clientId = payosSection["ClientId"] ?? string.Empty;
            var apiKey = payosSection["ApiKey"] ?? string.Empty;
            var checksumKey = payosSection["ChecksumKey"] ?? string.Empty;
            return new PayOSClient(clientId, apiKey, checksumKey);
        });

        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<IPayOSPaymentService, PayOSPaymentService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<ISubscriptionAccessService, SubscriptionAccessService>();
        return services;
    }

    public static Task SeedEduChatbotIdentityAsync(this IServiceProvider serviceProvider)
    {
        return IdentitySeeder.SeedAsync(serviceProvider);
    }
}
