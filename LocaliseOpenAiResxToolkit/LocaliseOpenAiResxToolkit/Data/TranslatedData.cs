namespace LocaliseOpenAiResxToolkit.Data;

public class TranslatedData
{
    public TranslatedData(string translatedResourcesFilePath, string translatedLanguageCode, IReadOnlyDictionary<string, string> translatedResources)
    {
        TranslatedResourcesFilePath = translatedResourcesFilePath;
        TranslatedLanguageCode = translatedLanguageCode;
        TranslatedResources = translatedResources;
    }

    public string TranslatedResourcesFilePath { get; init; }
    public string TranslatedLanguageCode { get; init; }
    
    public IReadOnlyDictionary<string, string> TranslatedResources { get; init; }
}