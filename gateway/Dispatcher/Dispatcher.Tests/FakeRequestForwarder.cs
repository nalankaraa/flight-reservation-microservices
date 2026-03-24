using Dispatcher.Application.Forwarding;

namespace Dispatcher.Tests;

public class FakeRequestForwarder : IRequestForwarder
{
    public Task<string> ForwardAsync(string targetUrl)
    {
        return Task.FromResult("Flights forwarded successfully");
    }
}