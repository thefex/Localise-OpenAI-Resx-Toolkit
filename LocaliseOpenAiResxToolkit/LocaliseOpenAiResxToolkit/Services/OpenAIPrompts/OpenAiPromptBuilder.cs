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
    }

    public OpenAiPrompt BuildSystemMessage(string targetLanguageCode)
    {
        const string systemPrompt =
            "Following data will be provided list of:\n{ \"key\" : \"text value\" }.\n\nYour task is to translate " +
            "\"key\" value into the language represented by following Culture Info \"[CULTUREINFO]\".\n\n" +
            "You should never, under any circumstances! output anything other than list of:" +
            "\n{ \"key\" : \"translated text value using [CULTUREINFO] culture info\" }\n\n" +
            "If provided content is not a valid input - not something you can translate with key value pair" +
            " - display \"ERROR_INVALID_INPUT\". ";

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
            userPromptBuilder.Append("{ \"" + item.Key + "\" : \"" + item.Value + "\" }, " + Environment.NewLine);

        var prompt = userPromptBuilder.ToString()
            .Substring(0, userPromptBuilder.Length - ", ".Length - Environment.NewLine.Length);

        return new OpenAiPrompt()
        {
            IsSystemPrompt = false,
            Prompt = prompt
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
                .ToHashSet();

        return baselineResources
            .Resources
            .Where(x => keysToTranslate.Contains(x.Key))
            .ToImmutableDictionary();
    }
}