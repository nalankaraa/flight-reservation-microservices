using Dispatcher.Application.Forwarding;

namespace Dispatcher.Infrastructure.Http;

public class HttpRequestForwarder : IRequestForwarder
{
    private readonly HttpClient _httpClient;

    public HttpRequestForwarder(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> ForwardAsync(string targetUrl)
    {
        return await _httpClient.GetStringAsync(targetUrl);
    }
}