using FluentValidation;
using LocaliseOpenAiResxToolkit.Data;

namespace LocaliseOpenAiResxToolkit.Services.Validators;

public class ApplicationParametersValidator : AbstractValidator<ApplicationParameters>
{
    public ApplicationParametersValidator()
    {
        RuleFor(x => x.OpenAiKey)
            .NotEmpty();
        
        RuleFor(x => x.BaseTranslationResxFile)
            .NotEmpty()
            .Must(x => x.EndsWith(".resx"))
            .Must(x => System.IO.Path.Exists(x)).WithMessage("Base Translation Resx File path does not exist.");
        
        RuleFor(x => x.ResxFilesDirectoryPath)
            .NotEmpty()
            .Must(x => System.IO.Directory.Exists(x)).WithMessage("Resx Files Directory path does not exist.");
    }
}