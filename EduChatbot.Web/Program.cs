using EduChatbot.Business;
using EduChatbot.Web.Hubs;
using EduChatbot.Business.Services;
using EduChatbot.Web.Services;
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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.MapHub<AdminHub>("/adminHub");

await app.Services.SeedEduChatbotIdentityAsync();

app.Run();
