namespace LocaliseOpenAiResxToolkit;

public class AppConstants
{
    private AppConstants()
    {
        
    }
    
    // 4096 max token count, -350 - so output fit in one output
    public const int OpenAiMaximalTokenCount = 4096 - 350;
}