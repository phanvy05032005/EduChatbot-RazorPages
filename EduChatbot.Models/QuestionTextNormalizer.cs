using System;
using System.Security.Cryptography;
using System.Text;

namespace EduChatbot.Models;

public static class QuestionTextNormalizer
{
    public static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var sb = new StringBuilder();
        foreach (char c in text)
        {
            if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }
        return sb.ToString();
    }

    public static string ComputeHash(string text)
    {
        var normalized = Normalize(text);
        if (string.IsNullOrEmpty(normalized)) return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(normalized);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
