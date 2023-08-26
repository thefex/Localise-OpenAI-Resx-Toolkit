using System.Text.Json.Serialization;

namespace LocaliseOpenAiResxToolkit.Data.Rest;

public class CompletionMessage
{
    [JsonPropertyName("role")] public string Role { get; set; }

    [JsonPropertyName("content")] public string Content { get; set; }
}