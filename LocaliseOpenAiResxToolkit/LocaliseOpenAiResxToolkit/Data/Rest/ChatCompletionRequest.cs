using System.Text.Json.Serialization;

namespace LocaliseOpenAiResxToolkit.Data.Rest;

public class ChatCompletionRequest
{
    [JsonPropertyName("model")] public string Model { get; set; } = "gpt-3.5-turbo";

    [JsonPropertyName("messages")] public IEnumerable<CompletionMessage> Messages { get; set; }
}