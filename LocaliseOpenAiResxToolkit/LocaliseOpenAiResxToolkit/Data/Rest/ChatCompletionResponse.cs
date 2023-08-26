using System.Text.Json.Serialization;

namespace LocaliseOpenAiResxToolkit.Data.Rest;

public class ChatCompletionResponse
{
    [JsonPropertyName("id")] public string Id { get; set; }

    [JsonPropertyName("object")] public string Object { get; set; }

    [JsonPropertyName("created")] public long Created { get; set; }

    [JsonPropertyName("choices")] public List<ChatCompletionChoice> Choices { get; set; }

    [JsonPropertyName("usage")] public CompletionUsage CompletionUsage { get; set; }
}