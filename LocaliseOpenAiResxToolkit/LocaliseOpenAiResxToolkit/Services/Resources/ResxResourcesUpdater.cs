using System.Resources.NetStandard;
using LocaliseOpenAiResxToolkit.Data;

namespace LocaliseOpenAiResxToolkit.Services.Resources;

public class ResxResourcesUpdater
{
    private readonly Dictionary<string, Dictionary<string, string>> _translatedDataToBeCommited = new Dictionary<string, Dictionary<string, string>>();

    public void AddTranslatedData(TranslatedData translatedData)
    {
        if (!_translatedDataToBeCommited.ContainsKey(translatedData.TranslatedResourcesFilePath))
            _translatedDataToBeCommited.Add(translatedData.TranslatedResourcesFilePath, new Dictionary<string, string>());
        
        foreach(var item in translatedData.TranslatedResources)
            _translatedDataToBeCommited[translatedData.TranslatedResourcesFilePath].Add(item.Key, item.Value);
    }
    
    public void CleanStagedChanges() => _translatedDataToBeCommited.Clear();

    public void CommitChangesToResourceFiles()
    {
        foreach (var resxFile in _translatedDataToBeCommited)
        {
            using (var resxWriter = new ResXResourceWriter(resxFile.Key))
            {
                resxWriter.AddResource(resxFile.Key, resxFile.Value);
                resxWriter.Generate();
            }
        }
        
        CleanStagedChanges();
    }
}