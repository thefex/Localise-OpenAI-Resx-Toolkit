namespace LocaliseOpenAiResxToolkit.Services.Refit;

public class DefaultHttpDelegatingHandler : DelegatingHandler
{
    public DefaultHttpDelegatingHandler() : base(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMilliseconds(30),
        ActivityHeadersPropagator = null
    })
    {
    }
}