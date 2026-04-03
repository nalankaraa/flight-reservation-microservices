using FluentAssertions;
using ReservationService.Application.Dtos;
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
        var service = new ReservationAppService(repository, availabilityClient);

        var result = await service.CreateAsync(new CreateReservationDto
        {
            FlightId = "flight-1",
            PassengerName = "Ali",
            SeatNumber = "10B"
        }, "user-1", "Bearer token");

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("SeatConflict");
        availabilityClient.ReleaseCallCount.Should().Be(1);
    }
}
