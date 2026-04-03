using System.Net;
using System.Text.Json;
using ReservationService.Infrastructure.Clients;

namespace ReservationService.Tests;

public class AvailabilityApiClientContractTests
{
    [Fact]
    public async Task LockSeatAsync_Should_Send_Put_Request_To_Hold_Endpoint_With_Auth_Header_And_Body()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://availabilityservice:8080/")
        };
        var client = new AvailabilityApiClient(httpClient);

        var result = await client.LockSeatAsync("flight-1", "1A", 10, "Bearer token-123");

        Assert.True(result.Success);
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Put, handler.LastRequest!.Method);
        Assert.Equal("http://availabilityservice:8080/api/availability/flight-1/seats/1A/hold", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("Bearer token-123", handler.LastRequest.Headers.Authorization!.ToString());

        var payload = await handler.LastRequest.Content!.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        Assert.Equal(10, document.RootElement.GetProperty("holdMinutes").GetInt32());
    }

    [Fact]
    public async Task ConfirmSeatAsync_Should_Send_Put_Request_To_Reservation_Endpoint()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://availabilityservice:8080/")
        };
        var client = new AvailabilityApiClient(httpClient);

        var result = await client.ConfirmSeatAsync("flight-9", "12C", "Bearer token-456");

        Assert.True(result.Success);
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Put, handler.LastRequest!.Method);
        Assert.Equal("http://availabilityservice:8080/api/availability/flight-9/seats/12C/reservation", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("Bearer token-456", handler.LastRequest.Headers.Authorization!.ToString());
        Assert.Null(handler.LastRequest.Content);
    }

    [Fact]
    public async Task ReleaseSeatAsync_Should_Send_Delete_Request_To_Hold_Endpoint()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://availabilityservice:8080/")
        };
        var client = new AvailabilityApiClient(httpClient);

        await client.ReleaseSeatAsync("flight-4", "7D", "Bearer token-789");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Delete, handler.LastRequest!.Method);
        Assert.Equal("http://availabilityservice:8080/api/availability/flight-4/seats/7D/hold", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("Bearer token-789", handler.LastRequest.Headers.Authorization!.ToString());
    }

    [Fact]
    public async Task LockSeatAsync_Should_Return_Conflict_When_Availability_Service_Returns_409()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.Conflict));
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://availabilityservice:8080/")
        };
        var client = new AvailabilityApiClient(httpClient);

        var result = await client.LockSeatAsync("flight-1", "1A", 10, "Bearer token");

        Assert.False(result.Success);
        Assert.True(result.IsConflict);
        Assert.False(result.IsServiceUnavailable);
    }

    [Fact]
    public async Task ConfirmSeatAsync_Should_Return_ServiceUnavailable_When_Downstream_Request_Fails()
    {
        var handler = new ThrowingHandler();
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://availabilityservice:8080/")
        };
        var client = new AvailabilityApiClient(httpClient);

        var result = await client.ConfirmSeatAsync("flight-1", "1A", "Bearer token");

        Assert.False(result.Success);
        Assert.False(result.IsConflict);
        Assert.True(result.IsServiceUnavailable);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = await CloneAsync(request);
            return _responseFactory(request);
        }

        private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri);

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content is not null)
            {
                var content = await request.Content.ReadAsStringAsync();
                clone.Content = new StringContent(content);

                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return clone;
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Availability service is unavailable.");
        }
    }
}
