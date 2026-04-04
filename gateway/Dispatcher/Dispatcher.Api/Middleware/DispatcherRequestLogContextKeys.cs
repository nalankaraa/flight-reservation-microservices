namespace Dispatcher.Api.Middleware;

public static class DispatcherRequestLogContextKeys
{
    public const string ResolvedRoute = "Dispatcher.ResolvedRoute";
    public const string TargetService = "Dispatcher.TargetService";
    public const string ErrorMessage = "Dispatcher.ErrorMessage";
}
