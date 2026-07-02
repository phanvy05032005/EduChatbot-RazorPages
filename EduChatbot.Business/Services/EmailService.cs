using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EduChatbot.Business.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public EmailService(ILogger<EmailService> logger, IWebHostEnvironment env, IConfiguration config, HttpClient httpClient)
    {
        _logger = logger;
        _env = env;
        _config = config;
        _httpClient = httpClient;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        _logger.LogInformation("Sending email to {ToEmail} with Subject: {Subject}", toEmail, subject);

        bool emailSentSuccessfully = false;

        // Try sending via Brevo API if configured
        var brevoSection = _config.GetSection("Brevo");
        if (brevoSection.Exists() && !string.IsNullOrEmpty(brevoSection["ApiKey"]))
        {
            try
            {
                var apiKey = brevoSection["ApiKey"];
                var senderEmail = brevoSection["SenderEmail"];
                var senderName = brevoSection["SenderName"] ?? "EduChatbot System";

                if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(senderEmail))
                {
                    var payload = new
                    {
                        sender = new { name = senderName, email = senderEmail },
                        to = new[] { new { email = toEmail } },
                        subject = subject,
                        htmlContent = body
                    };

                    var requestJson = JsonSerializer.Serialize(payload);
                    using (var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email"))
                    {
                        request.Headers.Add("api-key", apiKey);
                        request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                        var response = await _httpClient.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation("Email successfully sent via Brevo API to {ToEmail}.", toEmail);
                            emailSentSuccessfully = true;
                        }
                        else
                        {
                            var responseContent = await response.Content.ReadAsStringAsync();
                            _logger.LogWarning("Failed to send email via Brevo API. Status: {StatusCode}, Content: {ResponseContent}", 
                                response.StatusCode, responseContent);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Brevo section is configured but missing ApiKey or SenderEmail.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via Brevo API to {ToEmail}. Trying SMTP fallback...", toEmail);
            }
        }

        // Try sending via SMTP if configured and not already sent
        if (!emailSentSuccessfully)
        {
            var smtpSection = _config.GetSection("Smtp");
            if (smtpSection.Exists())
            {
                try
                {
                    var host = smtpSection["Host"] ?? "smtp.gmail.com";
                    var port = int.Parse(smtpSection["Port"] ?? "587");
                    var enableSsl = bool.Parse(smtpSection["EnableSsl"] ?? "true");
                    var username = smtpSection["Username"];
                    var password = smtpSection["Password"];
                    var senderEmail = smtpSection["SenderEmail"] ?? username;
                    var senderName = smtpSection["SenderName"] ?? "EduChatbot System";

                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(senderEmail))
                    {
                        using (var client = new SmtpClient(host, port))
                        {
                            client.EnableSsl = enableSsl;
                            client.UseDefaultCredentials = false;
                            client.Credentials = new NetworkCredential(username, password);

                            var mailMessage = new MailMessage
                            {
                                From = new MailAddress(senderEmail, senderName),
                                Subject = subject,
                                Body = body,
                                IsBodyHtml = true
                            };
                            mailMessage.To.Add(toEmail);

                            await client.SendMailAsync(mailMessage);
                            _logger.LogInformation("Email successfully sent via SMTP to {ToEmail}.", toEmail);
                            emailSentSuccessfully = true;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("SMTP is configured but missing Username, Password, or SenderEmail.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send email via SMTP to {ToEmail}. Falling back to file log.", toEmail);
                }
            }
            else
            {
                _logger.LogInformation("SMTP/Brevo not configured or failed. Falling back to file log.");
            }
        }

        // Always log to file as a backup or if SMTP failed/was not configured
        try
        {
            var emailLogDirectory = Path.Combine(_env.ContentRootPath, "email-logs");
            if (!Directory.Exists(emailLogDirectory))
            {
                Directory.CreateDirectory(emailLogDirectory);
            }

            var safeEmail = toEmail.Replace("@", "_").Replace(".", "_");
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            var fileName = Path.Combine(emailLogDirectory, $"{safeEmail}-{timestamp}.txt");

            var emailContent = $@"To: {toEmail}
Date: {DateTime.UtcNow}
Subject: {subject}
SentViaSmtp: {emailSentSuccessfully}
--------------------------------------------------
{body}
";
            await File.WriteAllTextAsync(fileName, emailContent);
            _logger.LogInformation("Email content logged to file: {FilePath}", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write email backup log to file for user {ToEmail}.", toEmail);
        }
    }
}

