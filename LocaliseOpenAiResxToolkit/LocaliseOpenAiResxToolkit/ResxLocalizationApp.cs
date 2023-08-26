using FluentValidation.Results;
using LocaliseOpenAiResxToolkit.Data;
using LocaliseOpenAiResxToolkit.Services.Resources;
using LocaliseOpenAiResxToolkit.Services.ResourcesProvider;
using LocaliseOpenAiResxToolkit.Services.Translations;
using LocaliseOpenAiResxToolkit.Services.Validators;
using LocaliseOpenAiResxToolkit.Utilities;
using Refit.Insane.PowerPack.Services;

namespace LocaliseOpenAiResxToolkit;

public class ResxLocalizationApp : IDisposable
{
    private readonly ApplicationParameters _applicationParameters;
    private readonly OpenAITranslationsProvider _openAiTranslationsProvider;
    private readonly ResxResourcesProvider _resxResourcesProvider;
    private readonly ResxResourcesUpdater _resxResourcesUpdater;
    private IDisposable? _openAiTranslationProviderSubscription;
    public ResxLocalizationApp(IRestService restService, ApplicationParameters applicationParameters)
    {
        _applicationParameters = applicationParameters;
        _resxResourcesProvider = new();
        _resxResourcesUpdater = new();
        _openAiTranslationsProvider = new OpenAITranslationsProvider(restService, applicationParameters);
    }

    public void Initialize()
    {
        if (_openAiTranslationProviderSubscription != null)
            return;
        
        _openAiTranslationProviderSubscription = _openAiTranslationsProvider.TranslationProvided.Subscribe(translatedData => _resxResourcesUpdater.AddTranslatedData(translatedData));
    }

    public async Task<LayerResponse> Execute()
    {
        var paramsValidationResult = ValidateApplicationParameters(_applicationParameters);

        if (!paramsValidationResult.IsSuccess)
            return paramsValidationResult;

        _resxResourcesUpdater.CleanStagedChanges();
        
        var resources = _resxResourcesProvider.GetResourcesData(_applicationParameters);

        try
        {
            var translateResponse = await _openAiTranslationsProvider.Translate(resources);
            return translateResponse;
        }
        finally
        {
            _resxResourcesUpdater.CommitChangesToResourceFiles();
        }
    }

    private void OnAppStarted()
    {
        
    }
    
    private LayerResponse ValidateApplicationParameters(ApplicationParameters applicationParameters)
    {
        ApplicationParametersValidator validator = new ApplicationParametersValidator();
        var paramsValidationResult = validator.Validate(applicationParameters);

        var validationFailedLayerResponse = LayerResponse.Build();
        if (!paramsValidationResult.IsValid)
        {
            foreach (var error in paramsValidationResult.Errors)
                validationFailedLayerResponse.AddErrorMessage(error.ErrorMessage);
        }

        return validationFailedLayerResponse;
    }

    public void Dispose()
    {
        _openAiTranslationProviderSubscription?.Dispose();
        _openAiTranslationProviderSubscription = null;
    }
}