using System.Text.Json.Serialization;

namespace LocaliseOpenAiResxToolkit.Data.Rest;

public class ChatCompletionChoice
{
    [JsonPropertyName("index")] public long Index { get; set; }

    [JsonPropertyName("message")] public CompletionMessage CompletionMessage { get; set; }

    [JsonPropertyName("finish_reason")] public string FinishReason { get; set; }
}