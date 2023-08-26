using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using System.Text;
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
        var baseResourcesData = resourcesDatas.Single(x => x.IsBaseResourcesData);
        var resourcesToBeTranslated = resourcesDatas.Where(x => !x.IsBaseResourcesData).ToList();

        ConsoleLogger.Instance.LogSuccess(nameof(OpenAITranslationsProvider) + ": using base resx file, " + baseResourcesData.FilePath);
        StringBuilder targetLanguageCodes = new StringBuilder();
        foreach(var item in resourcesToBeTranslated)
            targetLanguageCodes.Append(item.LanguageCode + " ");
        ConsoleLogger.Instance.LogSuccess(nameof(OpenAITranslationsProvider) + ": translations will be provided to following cultures - " + targetLanguageCodes);
        
        
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

                if (translationsResponse.Result.TranslatedResources.Any())
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
        var userPrompts = _openAiPromptBuilder
            .BuildUserPrompts(baseResourcesData, itemToBeTranslated)
            .ToList();

        if (!userPrompts.Any())
        {
            ConsoleLogger.Instance.LogInfo(nameof(OpenAITranslationsProvider) + ": skipping culture, " + itemToBeTranslated.LanguageCode + " - looks like all keys are already translated. Good job!");
            return LayerResponse<TranslatedData>.Build(
                new TranslatedData(
                    itemToBeTranslated.FilePath,
                    itemToBeTranslated.LanguageCode, 
                    new ReadOnlyDictionary<string, string>(new Dictionary<string, string>())));
        }

        Dictionary<string, string> translatedMap = new Dictionary<string, string>();
        
        foreach (var userPrompt in userPrompts)
        {
            var completionMessages = new List<CompletionMessage>
            {
                new() { Role = "system", Content = systemPrompt.Prompt },
            };
            
            completionMessages.Add(new() { Role = "user", Content = userPrompt.Prompt });
            var chatCompletionRequest = new ChatCompletionRequest { Messages = completionMessages };
            
            var bearerToken = $"Bearer {_applicationParameters.OpenAiKey}";

            var tokenCount = _openAiPromptBuilder.GetTokenCount(new []{ userPrompt }.Concat(new[] { systemPrompt }));
            ConsoleLogger.Instance.LogInfo(nameof(OpenAITranslationsProvider) + ": please wait, translating , "  + itemToBeTranslated.LanguageCode + ", " + tokenCount + " tokens processing. If token amount is high this might take up to few minutes ");
            var chatCompletionResponse =
                await _restService.Execute<IOpenAiApi, ChatCompletionResponse>(api => api.GetCompletion(chatCompletionRequest, bearerToken, default));

            if (!chatCompletionResponse.IsSuccess)
                return new LayerResponse<TranslatedData>().AddErrorMessage("Language: " + itemToBeTranslated.LanguageCode +
                                                                           " failed to translate with following error: " +
                                                                           chatCompletionResponse.FormattedErrorMessages);

            var openAiChoiceResponse = chatCompletionResponse.Results.Choices.First();
            int translatedTextIndex = 0;
            
            try
            {
                var translatedTexts = openAiChoiceResponse
                    .CompletionMessage
                    .Content
                    .Split("$$$")
                    .Where(x => !string.IsNullOrEmpty(x.Trim()))
                    .ToList();

                for (int i = 0; i < translatedTexts.Count(); ++i)
                {
                    translatedMap.Add(userPrompt.KeysInSortedOrder.ElementAt(translatedTextIndex), translatedTexts[translatedTextIndex].Trim());
                    translatedTextIndex++;
                }
            }
            catch (Exception e)
            {
                ConsoleLogger.Instance.LogError(
                    "Failed to deserialize part of OpenAI response for language: " + itemToBeTranslated.LanguageCode + " with following error: " + e.Message,
                    "You can try again later",
                    "Invalid response: " + openAiChoiceResponse.CompletionMessage.Content
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