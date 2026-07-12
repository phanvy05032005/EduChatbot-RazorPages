namespace EduChatbot.Business.Services;

public class EmbeddingSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "openai/text-embedding-3-small";

    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1/embeddings";

    public int Dimensions { get; set; } = 1536;
}
