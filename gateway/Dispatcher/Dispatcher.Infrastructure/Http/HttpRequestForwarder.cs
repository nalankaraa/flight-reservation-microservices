using Dispatcher.Application.Forwarding;

namespace Dispatcher.Infrastructure.Http;

public class HttpRequestForwarder : IRequestForwarder
{
    private readonly HttpClient _httpClient;

    public HttpRequestForwarder(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<HttpResponseMessage> ForwardAsync(
        string method,
        string targetUrl,
        Dictionary<string, string> headers,
        Stream body)
    {
        var request = new HttpRequestMessage(
            new HttpMethod(method),
            targetUrl
        );

        if (body != null && HttpMethodAllowsBody(method))
        {
            request.Content = new StreamContent(body);
        }

        foreach (var header in headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (TryAddContentHeader(request, header))
            {
                continue;
            }

            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
            {
                request.Content ??= new StreamContent(Stream.Null);
                request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
    }

    private static bool TryAddContentHeader(HttpRequestMessage request, KeyValuePair<string, string> header)
    {
        if (request.Content is null)
            return false;

        if (!header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
            return false;

        request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        return true;
    }

    private static bool HttpMethodAllowsBody(string method)
    {
        return method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("PATCH", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("DELETE", StringComparison.OrdinalIgnoreCase);
    }
}
