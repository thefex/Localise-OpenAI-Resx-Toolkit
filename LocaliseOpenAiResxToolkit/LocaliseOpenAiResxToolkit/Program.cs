using System.CommandLine;
using System.CommandLine.Binding;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using LocaliseOpenAiResxToolkit.Data;
using LocaliseOpenAiResxToolkit.Services.Logger;
using LocaliseOpenAiResxToolkit.Services.Refit;
using Newtonsoft.Json;
using Refit.Insane.PowerPack.Attributes;
using Refit.Insane.PowerPack.Services;

namespace LocaliseOpenAiResxToolkit
{
    public class Program
    {
        public static async Task<int> Main(params string[] args)
        {
            RootCommand rootCommand = new RootCommand(
                description: "Localise Resx with OpenAI GPT by Przemyslaw Raciborski [thefex].");
            var openaiTokenOption = new Option<string>(
                aliases: new string[] { "--openai-token" }
                , description: "OpenAI API key"
                , getDefaultValue: () => string.Empty    
            );
            
            var resxFilesDirectoryOption = new Option<string>(
                aliases: new string[] { "--resx-files-directory-path" }
                , description: "All resx files from provided directory will be translated."
                , getDefaultValue: () => string.Empty
            );
            
            var baseTranslationResxFileOption = new Option<string>(
                aliases: new string[] { "--base-resx-file-path" }
                , description: "Base resx file - all translations will be based on this file. The preferred is a path to the english resx file."
                , getDefaultValue: () => string.Empty    
            );

            var defaultResxCultureInfo = new Option<string>(
                aliases: new string[] { "--default-resx-culture-info" }
                , description: "Default Resx Culture Info you set in your project. If you do not provide anything, we will use en-US"
                , getDefaultValue: () => "en-US"
            );
            
            rootCommand.AddOption(openaiTokenOption);
            rootCommand.AddOption(baseTranslationResxFileOption);
            rootCommand.AddOption(resxFilesDirectoryOption);
            rootCommand.AddOption(defaultResxCultureInfo);

            rootCommand.SetHandler(Handle, openaiTokenOption, baseTranslationResxFileOption, resxFilesDirectoryOption, defaultResxCultureInfo);
            return await rootCommand.InvokeAsync(args); 
        }

        private static async Task<int> Handle(string openAiToken, string baseTranslationResxFile, string resxFilesDirectoryPath, string defaultResxCultureInfo)
        {
            var applicationParameters = new ApplicationParameters(openAiToken, baseTranslationResxFile, resxFilesDirectoryPath)
            {
                DefaultResxCultureInfo = defaultResxCultureInfo
            };

            RestServiceBuilder builder =
                new RestServiceBuilder()
                    .WithAutoRetry();
            
            var resxLocalizationApp = new ResxLocalizationApp(builder.BuildRestService(
                new Dictionary<Type, DelegatingHandler>()
                {
                    { typeof(DefaultHttpDelegatingHandler), new HttpClientDiagnosticsHandler(new DefaultHttpDelegatingHandler()) }
                }, applicationParameters.GetType().Assembly), applicationParameters);
            resxLocalizationApp.Initialize();
            
            try
            {
                ConsoleLogger.Instance.LogInfo("thefex.ResxLocalizationApp", "Starting Resx Localization App...");
                
                var response = await resxLocalizationApp.Execute();

                if (response.IsSuccess)
                    return 0;

                ConsoleLogger.Instance.LogError(response.FormattedErrorMessage);
                return -1;
            }
            catch (Exception e)
            {
                ConsoleLogger.Instance.LogError(e.Message);
                return -2;
            }
            finally
            {
                resxLocalizationApp?.Dispose();
            }
        }
    }
}