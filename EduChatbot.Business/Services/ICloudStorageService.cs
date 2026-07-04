using System.IO;
using System.Threading.Tasks;

namespace EduChatbot.Business.Services;

public class CloudUploadResultDto
{
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty;
}

public interface ICloudStorageService
{
    Task<CloudUploadResultDto> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteFileAsync(string publicId);
}
