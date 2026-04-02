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
        var request = new HttpRequestMessage(
            new HttpMethod(method),
            targetUrl
        );

        // Header kopyalama (Host hariç)
        foreach (var header in headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                continue;

<<<<<<< Updated upstream
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
=======
            if (body != null && (method == "POST" || method == "PUT" || method == "PATCH" || method == "DELETE"))
            {
                request.Content = new StreamContent(body);
            }

            foreach (var header in headers)
            {
                if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    request.Content ??= new StreamContent(Stream.Null);
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return await _httpClient.SendAsync(request);
>>>>>>> Stashed changes
        }

        // Body varsa ekle
        if (body != null && (method == "POST" || method == "PUT"))
        {
            request.Content = new StreamContent(body);
        }
<<<<<<< Updated upstream

        var response = await _httpClient.SendAsync(request);
        return response;
=======
        catch (TaskCanceledException)
        {
            return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("Upstream service timed out.")
            };
        }
>>>>>>> Stashed changes
    }
}