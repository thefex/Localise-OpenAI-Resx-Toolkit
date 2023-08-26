using System.Collections;
using System.Resources.NetStandard;
using LocaliseOpenAiResxToolkit.Data;
using LocaliseOpenAiResxToolkit.Services.Logger;
using LocaliseOpenAiResxToolkit.Utilities;

namespace LocaliseOpenAiResxToolkit.Services.ResourcesProvider;

public class ResxResourcesProvider
{
    public IEnumerable<ResourcesData> GetResourcesData(ApplicationParameters applicationParameters)
    {
        var baselineTranslationFileName = Path.GetFileNameWithoutExtension(applicationParameters.BaseTranslationResxFile);
        var translationFilesStartPattern = baselineTranslationFileName.Split(".").First();
        
        var resxFiles = System.IO.Directory.GetFiles(applicationParameters.ResxFilesDirectoryPath, translationFilesStartPattern + "*.resx", System.IO.SearchOption.AllDirectories);

        foreach (var resxFile in resxFiles)
        {
            ConsoleLogger.Instance.LogInfo("ResxResourcesProvider: found file, " + resxFile);

            var splittedFileName =
                Path.GetFileNameWithoutExtension(resxFile)
                    .Split(".");

            string languageCode = applicationParameters.DefaultResxCultureInfo;
            
            // "sample resx file name MyResources.resx, MyResources.fr-FR.resx, etc... - no second dot => default culture info
            if (splittedFileName.Length > 1)
                languageCode = splittedFileName.Last();
            
            var resxReader = new ResXResourceReader(resxFile);

            var resources = new Dictionary<string, string>();
            foreach (DictionaryEntry entry in resxReader)
                resources.Add(entry.Key.ToString(), entry.Value.ToString());
            
            yield return new ResourcesData(resxFile, languageCode, resources)
            {
                IsBaseResourcesData = languageCode == applicationParameters.DefaultResxCultureInfo
            };
        }
    }
}