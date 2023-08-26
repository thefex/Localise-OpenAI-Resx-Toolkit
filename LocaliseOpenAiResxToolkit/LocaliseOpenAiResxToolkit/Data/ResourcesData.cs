namespace LocaliseOpenAiResxToolkit.Data;

public class ResourcesData
{
    public ResourcesData(string filePath, string languageCode, IReadOnlyDictionary<string, string> resources)
    {
        FilePath = filePath;
        LanguageCode = languageCode;
        Resources = resources;
    }

    public string FilePath { get; }
    public string LanguageCode { get; }

    public bool IsBaseResourcesData { get; set; }
    public IReadOnlyDictionary<string, string> Resources { get; }

    public override int GetHashCode() => LanguageCode.GetHashCode();
    
    public override bool Equals(object obj)
    {
        if (obj is not ResourcesData other)
            return false;

        return LanguageCode == other.LanguageCode;
    }
}