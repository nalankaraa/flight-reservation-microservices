using Dispatcher.Application.Forwarding;

namespace Dispatcher.Tests;

public class ThrowingRequestForwarder : IRequestForwarder
{
    public Task<HttpResponseMessage> ForwardAsync(
        string method,
        string targetUrl,
        Dictionary<string, string> headers,
        Stream body)
    {
        throw new HttpRequestException("Downstream service is unavailable");
    }
}