using Dispatcher.Application.Forwarding;
using System.Net.Http;

namespace Dispatcher.Infrastructure.Http;

public class HttpRequestForwarder : IRequestForwarder
{
    private readonly HttpClient _httpClient;

    public HttpRequestForwarder(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> ForwardAsync(
        string method,
        string targetUrl,
        Dictionary<string, string> headers,
        Stream body)
    {
        try
        {
            var request = new HttpRequestMessage(
                new HttpMethod(method),
                targetUrl
            );

            foreach (var header in headers)
            {
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                    continue;

                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (body != null && (method == "POST" || method == "PUT"))
            {
                request.Content = new StreamContent(body);
            }

            return await _httpClient.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.BadGateway)
            {
                Content = new StringContent("Upstream service is unavailable.")
            };
        }
    }
}
