using Dispatcher.Application.Forwarding;

namespace Dispatcher.Tests;

public class TimeoutRequestForwarder : IRequestForwarder
{
    public Task<HttpResponseMessage> ForwardAsync(
        string method,
        string targetUrl,
        Dictionary<string, string> headers,
        Stream body)
    {
        throw new TaskCanceledException("Downstream service timed out");
    }
}
