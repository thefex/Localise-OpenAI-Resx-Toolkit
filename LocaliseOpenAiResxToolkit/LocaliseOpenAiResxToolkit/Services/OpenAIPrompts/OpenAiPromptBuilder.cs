using System.Collections.Immutable;
using System.Text;
using LocaliseOpenAiResxToolkit.Data;
using LocaliseOpenAiResxToolkit.Services.Logger;

namespace LocaliseOpenAiResxToolkit.Services.OpenAIPrompts;

public class OpenAiPromptBuilder
{
    public IEnumerable<OpenAiPrompt> BuildUserPrompts(ResourcesData baseResources, ResourcesData targetTranslationResources)
    {
        var dictionaryToBeTranslated = GetDictionaryToBeTranslated(baseResources, targetTranslationResources);

        int currentTokenCount = 0;
        List<KeyValuePair<string, string>> _currentPromptKeySet = new List<KeyValuePair<string, string>>();
        
        int maximalTokenCount = AppConstants.OpenAiMaximalTokenCount - GetApproximationOfTokenCount(BuildSystemMessage(targetTranslationResources.LanguageCode).Prompt);
        
        foreach (var item in dictionaryToBeTranslated)
        {
            int tokenCount = GetApproximationOfTokenCount(
                "{ " + item.Key + " : " + item.Value + " }, " + Environment.NewLine
            );

            if (tokenCount > maximalTokenCount)
            {
                ConsoleLogger.Instance.LogInfo("OpenAiPromptBuilder: skipping prompt, translate it manually, too long: " + item.Key + " : " + item.Value);
                continue;
            }
            
            if (currentTokenCount + tokenCount > AppConstants.OpenAiMaximalTokenCount)
            {
                yield return BuildPromptToTranslateResource(_currentPromptKeySet);
                _currentPromptKeySet.Clear();
                currentTokenCount = 0;
            }

            currentTokenCount += tokenCount;
            _currentPromptKeySet.Add(new KeyValuePair<string, string>(item.Key, item.Value));
        }        
        
        if (_currentPromptKeySet.Any())
            yield return BuildPromptToTranslateResource(_currentPromptKeySet);
    }

    public int GetTokenCount(IEnumerable<OpenAiPrompt> prompts) => prompts.Sum(x => GetApproximationOfTokenCount(x.Prompt));

    public OpenAiPrompt BuildSystemMessage(string targetLanguageCode)
    {
        const string systemPrompt =
            "Following data will be provided list of texts separated by $$$.\n\nYour task is to translate texts into the language represented by following Culture Info \"[CULTUREINFO]\", separate those entries by $$$.\n";

        return new OpenAiPrompt()
        {
            Prompt = systemPrompt.Replace("[CULTUREINFO]", targetLanguageCode),
            IsSystemPrompt = true
        };
    }

    private OpenAiPrompt BuildPromptToTranslateResource(List<KeyValuePair<string, string>> currentPromptKeySet)
    {
        StringBuilder userPromptBuilder = new StringBuilder();

        foreach (var item in currentPromptKeySet)
            userPromptBuilder.Append(item.Value + "$$$");

        var prompt = userPromptBuilder.ToString()
            .Substring(0, userPromptBuilder.Length - "$$$".Length);

        return new OpenAiPrompt()
        {
            IsSystemPrompt = false,
            Prompt = prompt,
            KeysInSortedOrder = currentPromptKeySet.Select(x => x.Key).ToList()
        };
    }

    private int GetApproximationOfTokenCount(string input) => (int)Math.Ceiling(input.Length / 4.0);

    private IReadOnlyDictionary<string, string> GetDictionaryToBeTranslated(ResourcesData baselineResources, ResourcesData targetResources)
    {
        var keysToTranslate =
            baselineResources
                .Resources
                .Keys
                .Except(targetResources.Resources.Keys)
                .Select(key => new KeyValuePair<string, string>(key, baselineResources.Resources[key]));

        return
            keysToTranslate
                .ToImmutableDictionary();
    }
}