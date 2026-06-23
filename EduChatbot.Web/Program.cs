using EduChatbot.Business;
using EduChatbot.Business.Hubs;
using EduChatbot.Business.Services;
using Microsoft.Extensions.Options;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
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

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        await ConfirmPayOSWebhookAsync(app.Services, app.Logger);
    });
});

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
