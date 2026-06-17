namespace EduChatbot.Models;

/// <summary>
/// Kết quả tìm kiếm chunk kèm similarity score.
/// Dùng cho RAG pipeline để đánh giá mức độ liên quan của chunk với câu hỏi.
/// </summary>
public class ChunkSearchResult
{
    public DocumentChunk Chunk { get; set; } = null!;

    /// <summary>
    /// Cosine similarity score (0.0 - 1.0). Càng cao càng liên quan.
    /// Tính bằng 1 - CosineDistance.
    /// </summary>
    public double SimilarityScore { get; set; }
}
