using System.Text.Json.Serialization;

namespace LocaliseOpenAiResxToolkit.Data.Rest;

public class CompletionUsage
{
    [JsonPropertyName("prompt_tokens")] public long PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public long CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")] public long TotalTokens { get; set; }
}