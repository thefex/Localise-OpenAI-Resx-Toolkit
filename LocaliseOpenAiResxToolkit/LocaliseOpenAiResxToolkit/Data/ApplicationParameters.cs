namespace LocaliseOpenAiResxToolkit.Data;

public class ApplicationParameters 
{
    public ApplicationParameters(string openAiKey, string baseTranslationResxFile, string resxFilesDirectoryPath)
    {
        OpenAiKey = openAiKey;
        BaseTranslationResxFile = baseTranslationResxFile;
        ResxFilesDirectoryPath = resxFilesDirectoryPath;
    }

    public string OpenAiKey { get; init; }
    public string BaseTranslationResxFile { get; init; }
    public string ResxFilesDirectoryPath { get; init; }
    public string DefaultResxCultureInfo { get; set; }
}