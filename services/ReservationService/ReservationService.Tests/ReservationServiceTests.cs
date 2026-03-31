using FluentAssertions;
using ReservationService.Application.Dtos;
using ReservationAppService = ReservationService.Application.Services.ReservationService;
using Xunit;

namespace ReservationService.Tests;

public class ReservationServiceTests
{
    [Fact]
    public async Task CreateReservation_Should_Return_Reservation()
    {
        var repository = new FakeReservationRepository();
        var service = new ReservationAppService(repository);

        var request = new CreateReservationDto
        {
            FlightId = "flight-1",
            PassengerName = "Beyza",
            SeatNumber = "12A"
        };

        var result = await service.CreateAsync(request);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.FlightId.Should().Be("flight-1");
        result.PassengerName.Should().Be("Beyza");
        result.SeatNumber.Should().Be("12A");
    }

    [Fact]
    public async Task GetReservations_Should_Return_List()
    {
        var repository = new FakeReservationRepository();
        var service = new ReservationAppService(repository);

        await service.CreateAsync(new CreateReservationDto
        {
            FlightId = "flight-1",
            PassengerName = "Ali",
            SeatNumber = "10B"
        });

        var result = await service.GetAllAsync();

        result.Should().HaveCount(1);
        result.First().Success.Should().BeTrue();
    }
}