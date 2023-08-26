using Refit.Insane.PowerPack.Retry;

[assembly: RefitRetry(2)]

namespace LocaliseOpenAiResxToolkit.Services.Refit;

public static class RefitApiConstants
{
    public static class OpenAiApi
    {
        public const int OpenAiApiTimeoutInSeconds = 30;
        public const string Url = "https://api.openai.com/v1";
    }
}