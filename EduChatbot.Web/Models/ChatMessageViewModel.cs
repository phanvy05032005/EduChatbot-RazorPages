namespace EduChatbot.Web.Models;

public class ChatMessageViewModel
{
    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public List<ChatSourceViewModel> Sources { get; set; } = [];

    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// True nếu câu hỏi nằm ngoài phạm vi tài liệu.
    /// Frontend sẽ hiển thị style khác cho message này.
    /// </summary>
    public bool IsOutOfScope { get; set; }
}

public class ChatSourceViewModel
{
    public string Doc { get; set; } = string.Empty;

    public int Chunk { get; set; }

    /// <summary>Cosine similarity score (0.0 - 1.0)</summary>
    public double Score { get; set; }

    /// <summary>Document ID để link xem tài liệu gốc</summary>
    public int DocumentId { get; set; }

    /// <summary>Preview text ngắn (~150 chars) từ chunk content</summary>
    public string ChunkPreview { get; set; } = string.Empty;
}
