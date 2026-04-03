using Dispatcher.Infrastructure.Http;
using FluentAssertions;
using System.Net;
using System.Text;

namespace Dispatcher.Tests;

public class HttpRequestForwarderTests
{
    [Fact]
    public async Task ForwardAsync_Should_Preserve_Json_ContentType_For_Post_Request()
    {
        var handler = new RecordingHandler();
        var httpClient = new HttpClient(handler);
        var forwarder = new HttpRequestForwarder(httpClient);
        var body = new MemoryStream(Encoding.UTF8.GetBytes("{\"from\":\"IST\"}"));

        await forwarder.ForwardAsync(
            "POST",
            "http://flightservice:8080/api/flights",
            new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json"
            },
            body);

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Content.Should().NotBeNull();
        handler.LastRequest.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            });
        }
    }
}
