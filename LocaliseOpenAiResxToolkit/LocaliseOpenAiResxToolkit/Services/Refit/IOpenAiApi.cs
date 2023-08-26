using LocaliseOpenAiResxToolkit.Data.Rest;
using Refit;
using Refit.Insane.PowerPack.Attributes;

namespace LocaliseOpenAiResxToolkit.Services.Refit;

[ApiDefinition(RefitApiConstants.OpenAiApi.Url, RefitApiConstants.OpenAiApi.OpenAiApiTimeoutInSeconds, typeof(DefaultHttpDelegatingHandler))]
public interface IOpenAiApi
{
    [Post("/chat/completions")]
    Task<ChatCompletionResponse> GetCompletion(ChatCompletionRequest request, [Header("Authorization")] string authorization, CancellationToken cancellationToken = default);
}