using System.Collections;
using System.Resources.NetStandard;
using System.Text;
using LocaliseOpenAiResxToolkit.Data;
using LocaliseOpenAiResxToolkit.Services.Logger;

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
            using (var resxReader = new ResXResourceReader(resxFile.Key))
            {
                foreach (DictionaryEntry entry in resxReader)
                    resxFile.Value.TryAdd(entry.Key.ToString(), entry.Value.ToString());
            }
            
            using (var resxWriter = new ResXResourceWriter(resxFile.Key))
            {
                StringBuilder keys = new StringBuilder();
                foreach (var (key, value) in resxFile.Value)
                {
                    keys.Append(key + ", ");
                    resxWriter.AddResource(key, value);
                }
                
                ConsoleLogger.Instance.LogSuccess("ResxResourcesUpdater: updating file " + resxFile.Key + ", following keys were saved: " + keys);
                resxWriter.Generate();
            }
        }
        
        CleanStagedChanges();
    }
}