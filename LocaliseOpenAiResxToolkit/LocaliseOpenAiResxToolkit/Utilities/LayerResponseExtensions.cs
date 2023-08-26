using Refit.Insane.PowerPack.Data;

namespace LocaliseOpenAiResxToolkit.Utilities;

public static class LayerResponseExtensions
{
    public static LayerResponse CloneFailedResponse(this LayerResponse layerResponse)
    {
        var clonedResponse = LayerResponse.Build();

        foreach (var error in layerResponse.ErrorMessages)
            clonedResponse.AddErrorMessage(error);

        return clonedResponse;
    }

    public static LayerResponse<TResult> CloneFailedResponse<TResult>(this LayerResponse layerResponse)
    {
        if (layerResponse.IsSuccess)
            throw new InvalidOperationException("Can't clone success response with result!");

        var clonedResponse = LayerResponse<TResult>.BuildEmptyLayerResponse();

        foreach (var error in layerResponse.ErrorMessages)
            clonedResponse.AddErrorMessage(error);

        return clonedResponse;
    }

    public static LayerResponse BuildLayerResponse(this Refit.Insane.PowerPack.Data.Response response)
    {
        if (response.IsSuccess)
            return LayerResponse.Build();

        var layerResponse = new LayerResponse();

        if (!response.ErrorMessages.Any())
            layerResponse.AddErrorMessage("Error.");

        foreach (var error in response.ErrorMessages)
            layerResponse.AddErrorMessage(error);

        return layerResponse;
    }

    public static LayerResponse<TResult> BuildLayerResponse<TResult>(this Response<TResult> response)
    {
        if (response.IsSuccess)
            return LayerResponse<TResult>.Build(response.Results);

        var layerResponse = LayerResponse<TResult>.BuildEmptyLayerResponse();

        if (!response.ErrorMessages.Any())
            layerResponse.AddErrorMessage("Error.");

        foreach (var error in response.ErrorMessages)
            layerResponse.AddErrorMessage(error);

        return layerResponse;
    }
}