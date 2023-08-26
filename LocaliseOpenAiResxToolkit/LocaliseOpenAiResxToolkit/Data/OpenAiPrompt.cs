namespace LocaliseOpenAiResxToolkit.Data;

public class OpenAiPrompt
{
    public bool IsSystemPrompt { get; set; }
    
    public IEnumerable<string> KeysInSortedOrder { get; set; }
    public string Prompt { get; set; }
}