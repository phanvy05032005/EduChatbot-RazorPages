using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EduChatbot.Business.Services;

public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
}

public class CloudinaryStorageService : ICloudStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryStorageService(IOptions<CloudinarySettings> config)
    {
        var settings = config.Value;
        var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<CloudUploadResultDto> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueId = $"{Guid.NewGuid():N}";
        
        // Retain extension in public ID to ensure proper file serving (Condition 4)
        var fullPublicId = $"educhatbot/documents/{uniqueId}{extension}";

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            PublicId = fullPublicId
        };

        var uploadResult = await _cloudinary.UploadLargeAsync(uploadParams);
        if (uploadResult.Error != null)
        {
            throw new InvalidOperationException($"Cloudinary upload error: {uploadResult.Error.Message}");
        }

        return new CloudUploadResultDto
        {
            Url = uploadResult.SecureUrl.ToString(),
            PublicId = uploadResult.PublicId
        };
    }

    public async Task<bool> DeleteFileAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return false;
        }

        var deletionParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Raw
        };

        var result = await _cloudinary.DestroyAsync(deletionParams);
        return result.Result == "ok";
    }
}
