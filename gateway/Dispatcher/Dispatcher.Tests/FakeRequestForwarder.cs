using Dispatcher.Application.Forwarding;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Dispatcher.Tests;

public class FakeRequestForwarder : IRequestForwarder
{
    public Task<HttpResponseMessage> ForwardAsync(
        string method,
        string targetUrl,
        Dictionary<string, string> headers,
        Stream body)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Flights forwarded successfully", Encoding.UTF8, "application/json")
        };

        return Task.FromResult(response);
    }
}