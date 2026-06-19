namespace EduChatbot.Business.Services;

public class ChatSettings
{
    /// <summary>
    /// Ngưỡng similarity tối thiểu để coi chunk là liên quan.
    /// Chunks có score dưới ngưỡng này sẽ bị coi là không liên quan.
    /// Mặc định: 0.3
    /// </summary>
    public double SimilarityThreshold { get; set; } = 0.3;

    /// <summary>
    /// Message hiển thị khi câu hỏi nằm ngoài phạm vi tài liệu.
    /// </summary>
    public string OutOfScopeMessage { get; set; } =
        "Xin lỗi, câu hỏi của bạn nằm ngoài phạm vi tài liệu đã được upload cho môn học này. Vui lòng đặt câu hỏi liên quan đến nội dung tài liệu đã có.";
}
