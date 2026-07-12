using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EduChatbot.Business.Services;

public class LocalFileStorageService : ICloudStorageService
{
    private readonly string _uploadRoot;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
    {
        _uploadRoot = Path.Combine(env.WebRootPath, "uploads");
        Directory.CreateDirectory(_uploadRoot);
        _logger = logger;
    }

    public async Task<CloudUploadResultDto> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueId = $"{Guid.NewGuid():N}";
        var storedFileName = $"{uniqueId}{extension}";
        var filePath = Path.Combine(_uploadRoot, storedFileName);

        await using (var outputStream = File.Create(filePath))
        {
            await fileStream.CopyToAsync(outputStream);
        }

        _logger.LogInformation("File saved locally: {FilePath}", filePath);

        return new CloudUploadResultDto
        {
            Url = $"/uploads/{storedFileName}",
            PublicId = storedFileName
        };
    }

    public Task<bool> DeleteFileAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return Task.FromResult(false);

        var filePath = Path.Combine(_uploadRoot, publicId);
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                _logger.LogInformation("Local file deleted: {FilePath}", filePath);
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete local file: {FilePath}", filePath);
                return Task.FromResult(false);
            }
        }

        return Task.FromResult(false);
    }
}
