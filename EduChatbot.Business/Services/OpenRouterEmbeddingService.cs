using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace EduChatbot.Business.Services;

public class OpenRouterEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _openRouterSettings;
    private readonly EmbeddingSettings _embeddingSettings;

    public OpenRouterEmbeddingService(
        HttpClient httpClient,
        IOptions<OpenRouterSettings> openRouterSettings,
        IOptions<EmbeddingSettings> embeddingSettings)
    {
        _httpClient = httpClient;
        _openRouterSettings = openRouterSettings.Value;
        _embeddingSettings = embeddingSettings.Value;
    }

    public async Task<float[]> CreateEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(_openRouterSettings.ApiKey))
        {
            throw new InvalidOperationException("OpenRouter API key chưa được cấu hình.");
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Không thể tạo embedding cho nội dung rỗng.", nameof(text));
        }

        var requestBody = new
        {
            model = _embeddingSettings.Model,
            input = text.Trim(),
            encoding_format = "float",
            dimensions = _embeddingSettings.Dimensions
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _embeddingSettings.BaseUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openRouterSettings.ApiKey);

        using var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"OpenRouter embedding API lỗi HTTP {(int)response.StatusCode}: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var embedding = ReadEmbeddingFromResponse(doc.RootElement, responseBody);

        if (_embeddingSettings.Dimensions > 0 && embedding.Length != _embeddingSettings.Dimensions)
        {
            throw new InvalidOperationException(
                $"Embedding trả về {embedding.Length} chiều, khác cấu hình {_embeddingSettings.Dimensions} chiều.");
        }

        return embedding;
    }

    private static float[] ReadEmbeddingFromResponse(JsonElement root, string responseBody)
    {
        // OpenRouter có thể trả HTTP 200 nhưng body là error/model response khác schema.
        // Vì vậy không dùng GetProperty trực tiếp để tránh KeyNotFoundException khó hiểu.
        if (root.TryGetProperty("error", out var error))
        {
            throw new InvalidOperationException($"OpenRouter embedding API trả về lỗi: {SummarizeJson(error)}");
        }

        if (!root.TryGetProperty("data", out var data) ||
            data.ValueKind != JsonValueKind.Array ||
            data.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                $"OpenRouter embedding API không trả về trường data[0].embedding. Response: {SummarizeResponse(responseBody)}");
        }

        var firstItem = data[0];
        if (!firstItem.TryGetProperty("embedding", out var embeddingElement) ||
            embeddingElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                $"OpenRouter embedding API không trả về embedding dạng mảng. Response: {SummarizeResponse(responseBody)}");
        }

        return embeddingElement
            .EnumerateArray()
            .Select(value => value.GetSingle())
            .ToArray();
    }

    private static string SummarizeJson(JsonElement element)
    {
        return SummarizeResponse(element.GetRawText());
    }

    private static string SummarizeResponse(string responseBody)
    {
        const int maxLength = 1000;

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return "(empty response)";
        }

        return responseBody.Length <= maxLength
            ? responseBody
            : responseBody[..maxLength] + "...";
    }
}
