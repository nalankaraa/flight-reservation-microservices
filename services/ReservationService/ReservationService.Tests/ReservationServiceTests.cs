using FluentAssertions;
using ReservationAppService = ReservationService.Application.Services.ReservationService;
using Xunit;

namespace ReservationService.Tests;

public class ReservationServiceTests
{
    [Fact]
    public async Task CreateReservation_Should_Return_SeatConflict_When_Insert_Fails_With_Duplicate()
    {
        var repository = new FakeReservationRepository
        {
            ThrowDuplicateOnAdd = true
        };
        var availabilityClient = new FakeSeatAvailabilityClient();
        var flightClient = new FakeFlightDetailsClient();
        var paymentClient = new FakePaymentClient();
        var service = new ReservationAppService(repository, availabilityClient, flightClient, paymentClient);

        var result = await service.CreateAsync(
            ReservationRequestFactory.Create(),
            "user-1",
            "Bearer token");

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("SeatConflict");
        availabilityClient.ReleaseCallCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateReservation_Should_Create_Payment_When_Reservation_Succeeds()
    {
        var repository = new FakeReservationRepository();
        var availabilityClient = new FakeSeatAvailabilityClient();
        var flightClient = new FakeFlightDetailsClient { Price = 3200m };
        var paymentClient = new FakePaymentClient();
        var service = new ReservationAppService(repository, availabilityClient, flightClient, paymentClient);

        var result = await service.CreateAsync(
            ReservationRequestFactory.Create(),
            "user-1",
            "Bearer token");

        result.Success.Should().BeTrue();
        result.PaymentId.Should().Be("payment-1");
        result.PaymentStatus.Should().Be("Pending");
        paymentClient.CreateCallCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateReservation_Should_Roll_Back_When_Payment_Service_Is_Unavailable()
    {
        var repository = new FakeReservationRepository();
        var availabilityClient = new FakeSeatAvailabilityClient();
        var flightClient = new FakeFlightDetailsClient();
        var paymentClient = new FakePaymentClient
        {
            ShouldBeUnavailable = true
        };
        var service = new ReservationAppService(repository, availabilityClient, flightClient, paymentClient);

        var result = await service.CreateAsync(
            ReservationRequestFactory.Create(),
            "user-1",
            "Bearer token");

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("PaymentUnavailable");
        availabilityClient.ReleaseCallCount.Should().Be(1);
        (await repository.GetAllAsync()).Should().BeEmpty();
    }
}
