namespace LocaliseOpenAiResxToolkit.Services.Logger;

public class ConsoleLogger
{
    private static readonly Lazy<ConsoleLogger> _instance = new Lazy<ConsoleLogger>(() => new ConsoleLogger());

    private ConsoleLogger()
    {
        
    }
    
    public static ConsoleLogger Instance => _instance.Value;

    public void LogError(params string[] errors)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("----------------------------------");
        Console.WriteLine("Resx Localization App failed with following errors...");
        Console.WriteLine("----------------------------------");
        foreach(var error in errors)
            Console.WriteLine(error);
        Console.WriteLine("----------------------------------");
        Console.ResetColor();
    }

    public void LogInfo(params string[] info)
    {
        System.Console.ForegroundColor = ConsoleColor.Blue;
        foreach (var infoValue in info) 
            Console.WriteLine(infoValue);
        
        Console.WriteLine("----------------------------------");
        System.Console.ResetColor();
    }
    
    public void LogSuccess(params string[] success){
        System.Console.ForegroundColor = ConsoleColor.Green;

        foreach (var successValue in success) 
            System.Console.WriteLine(successValue);

        Console.WriteLine("----------------------------------");
        System.Console.ResetColor();
    }
}