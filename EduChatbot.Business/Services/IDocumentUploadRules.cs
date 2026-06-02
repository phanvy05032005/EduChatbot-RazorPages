namespace EduChatbot.Business.Services;

public interface IDocumentUploadRules
{
    long MaxFileSizeBytes { get; }

    IReadOnlyCollection<string> AllowedExtensions { get; }

    IReadOnlyCollection<string> AllowedStatuses { get; }

    bool IsAllowedExtension(string extension);

    bool IsAllowedStatus(string status);
}
