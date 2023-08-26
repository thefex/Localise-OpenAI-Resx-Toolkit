using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using LocaliseOpenAiResxToolkit.Data;
using LocaliseOpenAiResxToolkit.Data.Rest;
using LocaliseOpenAiResxToolkit.Services.Logger;
using LocaliseOpenAiResxToolkit.Services.OpenAIPrompts;
using LocaliseOpenAiResxToolkit.Services.Refit;
using LocaliseOpenAiResxToolkit.Utilities;
using Newtonsoft.Json;
using Refit.Insane.PowerPack.Services;

namespace LocaliseOpenAiResxToolkit.Services.Translations;

public class OpenAITranslationsProvider
{
    private readonly IRestService _restService;
    private readonly ApplicationParameters _applicationParameters;
    private readonly OpenAiPromptBuilder _openAiPromptBuilder = new();
    private readonly Subject<TranslatedData> _translatedDataSubject = new();

    public OpenAITranslationsProvider(IRestService restService, ApplicationParameters applicationParameters)
    {
        _restService = restService;
        _applicationParameters = applicationParameters;
    }
    
    public async Task<LayerResponse> Translate(IEnumerable<ResourcesData> resourcesDatas)
    {
        var baseResourcesData = resourcesDatas.First(x => x.IsBaseResourcesData);
        var resourcesToBeTranslated = resourcesDatas.Except(new [] { baseResourcesData }).ToList();

        LayerResponse layerResponse = new LayerResponse();

        foreach (var itemToBeTranslated in resourcesToBeTranslated)
        {
            try
            {
                var translationsResponse = await GetTranslationsThroughOpenAiApi(itemToBeTranslated, baseResourcesData);

                if (!translationsResponse.IsSuccess)
                {
                    foreach (var error in translationsResponse.ErrorMessages)
                        layerResponse.AddErrorMessage(error);
                    continue;
                }

                _translatedDataSubject.OnNext(translationsResponse.Result);
            }
            catch (Exception e)
            {
                layerResponse.AddErrorMessage("Language: " + itemToBeTranslated.LanguageCode + " failed to translate with following error: " + e.Message);
            }
        }

        return layerResponse;
    }

    private async Task<LayerResponse<TranslatedData>> GetTranslationsThroughOpenAiApi(ResourcesData itemToBeTranslated, ResourcesData baseResourcesData)
    {
        var systemPrompt = _openAiPromptBuilder.BuildSystemMessage(itemToBeTranslated.LanguageCode);
        var userPrompts = _openAiPromptBuilder.BuildUserPrompts(baseResourcesData, itemToBeTranslated);

        var completionMessages = new List<CompletionMessage>
        {
            new() { Role = "system", Content = systemPrompt.Prompt },
        };

        foreach (var userPrompt in userPrompts)
            completionMessages.Add(new() { Role = "user", Content = userPrompt.Prompt });

        var chatCompletionRequest = new ChatCompletionRequest { Messages = completionMessages };

        var bearerToken = $"Bearer {_applicationParameters.OpenAiKey}";
        var chatCompletionResponse =
            await _restService.Execute<IOpenAiApi, ChatCompletionResponse>(api => api.GetCompletion(chatCompletionRequest, bearerToken, default));

        if (!chatCompletionResponse.IsSuccess)
        {
            return new LayerResponse<TranslatedData>()
                .AddErrorMessage("Language: " + itemToBeTranslated.LanguageCode +
                                 " failed to translate with following error: " + chatCompletionResponse.FormattedErrorMessages);
        }

        Dictionary<string, string> translatedMap = new Dictionary<string, string>();
        foreach (var choice in chatCompletionResponse.Results.Choices)
        {
            var openAiChoiceResponse = choice.CompletionMessage.Content;

            try
            {
                var deserializedMap = JsonConvert.DeserializeObject<IDictionary<string, string>>(openAiChoiceResponse);

                foreach (var (key, value) in deserializedMap!)
                    translatedMap.Add(key, value);
            }
            catch (Exception e)
            {
                ConsoleLogger.Instance.LogError(
                        "Failed to deserialize part of OpenAI response for language: " + itemToBeTranslated.LanguageCode + " with following error: " + e.Message,
                        "You can try again later",
                        "Invalid response: " + choice.CompletionMessage.Content
                    );
            }
        }

        return LayerResponse<TranslatedData>.Build(new TranslatedData(
            itemToBeTranslated.FilePath,
            itemToBeTranslated.LanguageCode,
            new ReadOnlyDictionary<string, string>(translatedMap)
        ));
    }

    public IObservable<TranslatedData> TranslationProvided => _translatedDataSubject;

}