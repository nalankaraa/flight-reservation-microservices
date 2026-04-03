using System.Net;
using System.Net.Http.Json;
using Dispatcher.Application.Forwarding;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Dispatcher.Tests;

public class EndToEndWorkflowTests : IClassFixture<DispatcherWebApplicationFactory>
{
    private readonly DispatcherWebApplicationFactory _factory;

    public EndToEndWorkflowTests(DispatcherWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Dispatcher_Should_Execute_EndToEnd_Booking_And_Payment_Workflow()
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IRequestForwarder>();
                services.AddSingleton<IRequestForwarder, ScenarioRequestForwarder>();
            });
        }).CreateClient();

        var adminToken = await LoginAsync(client, "admin@system.local", "Admin123!");
        var customerToken = await LoginAsync(client, "customer@system.local", "Customer123!");

        var createFlightRequest = new HttpRequestMessage(HttpMethod.Post, "/api/flights")
        {
            Content = JsonContent.Create(new
            {
                From = "IST",
                To = "ESB",
                DepartureTime = DateTime.UtcNow.AddDays(2),
                ArrivalTime = DateTime.UtcNow.AddDays(2).AddHours(1),
                Price = 1850,
                AvailableSeatCount = 3
            })
        };
        createFlightRequest.Headers.Add("Authorization", $"Bearer {adminToken}");

        var createFlightResponse = await client.SendAsync(createFlightRequest);
        createFlightResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdFlight = await createFlightResponse.Content.ReadFromJsonAsync<FlightResponse>();
        createdFlight.Should().NotBeNull();

        var initialAvailabilityRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/availability/{createdFlight!.Id}");
        initialAvailabilityRequest.Headers.Add("Authorization", $"Bearer {customerToken}");

        var initialAvailabilityResponse = await client.SendAsync(initialAvailabilityRequest);
        initialAvailabilityResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var initialAvailability = await initialAvailabilityResponse.Content.ReadFromJsonAsync<AvailabilitySummaryResponse>();
        initialAvailability.Should().NotBeNull();
        initialAvailability!.TotalSeats.Should().Be(3);
        initialAvailability.AvailableSeats.Should().Be(3);
        initialAvailability.LockedSeats.Should().Be(0);

        var createReservationRequest = new HttpRequestMessage(HttpMethod.Post, "/api/reservations")
        {
            Content = JsonContent.Create(new
            {
                FlightId = createdFlight.Id,
                PassengerName = "Nalan Kara",
                SeatNumber = "1A"
            })
        };
        createReservationRequest.Headers.Add("Authorization", $"Bearer {customerToken}");

        var createReservationResponse = await client.SendAsync(createReservationRequest);
        createReservationResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdReservation = await createReservationResponse.Content.ReadFromJsonAsync<ReservationResponse>();
        createdReservation.Should().NotBeNull();
        createdReservation!.PaymentId.Should().NotBeNullOrWhiteSpace();
        createdReservation.PaymentStatus.Should().Be("Pending");

        var reservedAvailabilityRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/availability/{createdFlight.Id}");
        reservedAvailabilityRequest.Headers.Add("Authorization", $"Bearer {customerToken}");

        var reservedAvailabilityResponse = await client.SendAsync(reservedAvailabilityRequest);
        reservedAvailabilityResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reservedAvailability = await reservedAvailabilityResponse.Content.ReadFromJsonAsync<AvailabilitySummaryResponse>();
        reservedAvailability.Should().NotBeNull();
        reservedAvailability!.TotalSeats.Should().Be(3);
        reservedAvailability.AvailableSeats.Should().Be(2);
        reservedAvailability.LockedSeats.Should().Be(1);

        var getPaymentRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/payments/{createdReservation.PaymentId}");
        getPaymentRequest.Headers.Add("Authorization", $"Bearer {customerToken}");

        var getPaymentResponse = await client.SendAsync(getPaymentRequest);
        getPaymentResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var payment = await getPaymentResponse.Content.ReadFromJsonAsync<PaymentResponse>();
        payment.Should().NotBeNull();
        payment!.Status.Should().Be("Pending");
        payment.Amount.Should().Be(1850);

        var completePaymentRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/payments/{createdReservation.PaymentId}")
        {
            Content = JsonContent.Create(new
            {
                Status = "Completed"
            })
        };
        completePaymentRequest.Headers.Add("Authorization", $"Bearer {customerToken}");

        var completePaymentResponse = await client.SendAsync(completePaymentRequest);
        completePaymentResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getNotificationsRequest = new HttpRequestMessage(HttpMethod.Get, "/api/notifications/user/customer-1");
        getNotificationsRequest.Headers.Add("Authorization", $"Bearer {customerToken}");

        var getNotificationsResponse = await client.SendAsync(getNotificationsRequest);
        getNotificationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var notifications = await getNotificationsResponse.Content.ReadFromJsonAsync<List<NotificationResponse>>();
        notifications.Should().NotBeNull();
        notifications!.Should().ContainSingle();
        notifications[0].Type.Should().Be("PaymentCompleted");
        notifications[0].UserId.Should().Be("customer-1");
    }

    private static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>();
        payload.Should().NotBeNull();
        payload!.Token.Should().NotBeNullOrWhiteSpace();
        return payload.Token!;
    }

    private sealed class AuthResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
    }

    private sealed class FlightResponse
    {
        public string Id { get; set; } = default!;
        public decimal Price { get; set; }
    }

    private sealed class AvailabilitySummaryResponse
    {
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public int LockedSeats { get; set; }
    }

    private sealed class ReservationResponse
    {
        public string? PaymentId { get; set; }
        public string? PaymentStatus { get; set; }
    }

    private sealed class PaymentResponse
    {
        public decimal Amount { get; set; }
        public string Status { get; set; } = default!;
    }

    private sealed class NotificationResponse
    {
        public string UserId { get; set; } = default!;
        public string Type { get; set; } = default!;
    }
}