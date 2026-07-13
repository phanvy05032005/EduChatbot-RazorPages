using EduChatbot.Business;
using EduChatbot.Business.Services;
using EduChatbot.Web.Hubs;
using EduChatbot.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.IO;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Load environment variables from .env file if it exists
var envPath = Path.Combine(Directory.GetCurrentDirectory(), "../.env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var value = parts[1].Trim();
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

// Add Environment Variables to configuration source to ensure they override appsettings.json
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Đăng ký toàn bộ Business + Data thông qua Business layer để Web không phụ thuộc trực tiếp Data layer.
builder.Services.AddEduChatbotApplication(builder.Configuration);
builder.Services.AddScoped<IRealtimeService, RealtimeService>();
builder.Services.AddScoped<IStudentRealtimeNotifier, StudentRealtimeNotifier>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Intercept requests for dynamic uploads to show friendly errors if they are missing (Condition 7 & 12)
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "";
    if (path.StartsWith("/uploads/documents/", StringComparison.OrdinalIgnoreCase))
    {
        var webHost = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
        var physicalPath = Path.Combine(webHost.WebRootPath, path.TrimStart('/'));
        if (!System.IO.File.Exists(physicalPath))
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("<html><head><meta charset='utf-8'/><title>Tài liệu không khả dụng</title><style>body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; display: flex; align-items: center; justify-content: center; height: 100vh; background-color: #12141c; color: #fff; margin: 0; } .card { background: #1a1d26; padding: 2rem; border-radius: 12px; border: 1px solid #2d313f; text-align: center; max-width: 450px; box-shadow: 0 4px 12px rgba(0,0,0,0.3); } h2 { color: #ff4a5a; margin-top: 0; } p { color: #a0aec0; line-height: 1.5; margin-bottom: 1.5rem; } .btn { display: inline-block; background: #007bff; color: #fff; text-decoration: none; padding: 0.6rem 1.5rem; border-radius: 20px; font-weight: 500; transition: background 0.2s; } .btn:hover { background: #0056b3; }</style></head><body><div class='card'><h2>Tài liệu không còn tồn tại</h2><p>Tài liệu gốc lưu trên máy chủ tạm thời của Render đã bị xóa do hệ thống khởi động lại. Vui lòng liên hệ giảng viên để tải lên lại tài liệu này.</p><a href='javascript:history.back()' class='btn'>Quay lại</a></div></body></html>");
            return;
        }
    }
    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapPost("/api/payment/payos/webhook", async (HttpRequest request, IPayOSPaymentService paymentService, ILogger<Program> logger) =>
{
    using var reader = new StreamReader(request.Body);
    var payload = await reader.ReadToEndAsync();

    try
    {
        var transaction = await paymentService.ProcessWebhookAsync(payload);
        if (transaction == null)
        {
            return Results.Ok(new
            {
                success = true,
                message = "Webhook verified but transaction not found locally (ignored)."
            });
        }
        return Results.Ok(new
        {
            success = true,
            orderCode = transaction.OrderCode,
            status = transaction.Status.ToString()
        });
    }
    catch (InvalidOperationException ex)
    {
        logger.LogWarning(ex, "PayOS webhook rejected.");
        return Results.BadRequest(new
        {
            success = false,
            message = ex.Message
        });
    }
});

app.MapRazorPages()
    .WithStaticAssets();

app.MapHub<AdminHub>("/adminHub");
app.MapHub<EduNotificationHub>("/notificationHub");

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        await ConfirmPayOSWebhookAsync(app.Services, app.Logger);
    });
});

await app.Services.MigrateDatabaseAsync();

await app.Services.SeedEduChatbotIdentityAsync();
await EduChatbot.Business.Services.SubscriptionSeeder.SeedAsync(app.Services);
app.Run();

static async Task ConfirmPayOSWebhookAsync(IServiceProvider services, ILogger logger)
{
    using var scope = services.CreateScope();
    var options = scope.ServiceProvider.GetRequiredService<IOptions<PayOSOptions>>().Value;

    if (!options.AutoConfirmWebhook || string.IsNullOrWhiteSpace(options.WebhookUrl))
    {
        logger.LogInformation(
            "Skip PayOS webhook confirmation. AutoConfirmWebhook={AutoConfirmWebhook}, WebhookUrlConfigured={WebhookUrlConfigured}",
            options.AutoConfirmWebhook,
            !string.IsNullOrWhiteSpace(options.WebhookUrl));
        return;
    }

    try
    {
        var client = scope.ServiceProvider.GetRequiredService<PayOS.PayOSClient>();
        await client.Webhooks.ConfirmAsync(options.WebhookUrl);
        logger.LogInformation("PayOS webhook confirmed successfully for {WebhookUrl}", options.WebhookUrl);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Unable to confirm PayOS webhook for {WebhookUrl}. Webhook validation failed. Please check your ngrok/public URL and PayOS credentials in config. The application will continue running.", options.WebhookUrl);
    }
}
