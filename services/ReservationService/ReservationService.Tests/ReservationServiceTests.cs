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
        var service = new ReservationAppService(repository, new FakeSeatAvailabilityClient());

        var request = new CreateReservationDto
        {
            FlightId = "flight-1",
            PassengerName = "Beyza",
            SeatNumber = "12A"
        };

        var result = await service.CreateAsync(request, "user-1", "Bearer token");

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.UserId.Should().Be("user-1");
        result.FlightId.Should().Be("flight-1");
        result.PassengerName.Should().Be("Beyza");
        result.SeatNumber.Should().Be("12A");
    }

    [Fact]
    public async Task GetReservations_Should_Return_List()
    {
        var repository = new FakeReservationRepository();
        var service = new ReservationAppService(repository, new FakeSeatAvailabilityClient());

        await service.CreateAsync(new CreateReservationDto
        {
            FlightId = "flight-1",
            PassengerName = "Ali",
            SeatNumber = "10B"
        }, "user-1", "Bearer token");

        var result = await service.GetAllAsync();

        result.Should().HaveCount(1);
        result.First().Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetMine_Should_Return_Only_Current_User_Reservations()
    {
        var repository = new FakeReservationRepository();
        var service = new ReservationAppService(repository, new FakeSeatAvailabilityClient());

        await service.CreateAsync(new CreateReservationDto
        {
            FlightId = "flight-1",
            PassengerName = "Ali",
            SeatNumber = "10B"
        }, "user-1", "Bearer token");

        await service.CreateAsync(new CreateReservationDto
        {
            FlightId = "flight-2",
            PassengerName = "Ayse",
            SeatNumber = "11C"
        }, "user-2", "Bearer token");

        var result = await service.GetMineAsync("user-1");

        result.Should().HaveCount(1);
        result[0].UserId.Should().Be("user-1");
        result[0].FlightId.Should().Be("flight-1");
    }

    [Fact]
    public async Task CreateReservation_Should_Return_Conflict_When_Seat_Is_Locked()
    {
        var repository = new FakeReservationRepository();
        var availabilityClient = new FakeSeatAvailabilityClient
        {
            LockShouldConflict = true
        };
        var service = new ReservationAppService(repository, availabilityClient);

        var result = await service.CreateAsync(new CreateReservationDto
        {
            FlightId = "flight-1",
            PassengerName = "Ali",
            SeatNumber = "10B"
        }, "user-1", "Bearer token");

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("SeatConflict");
    }

    [Fact]
    public async Task CreateReservation_Should_Roll_Back_When_Seat_Confirmation_Fails()
    {
        var repository = new FakeReservationRepository();
        var availabilityClient = new FakeSeatAvailabilityClient
        {
            ConfirmShouldBeUnavailable = true
        };
        var service = new ReservationAppService(repository, availabilityClient);

        var result = await service.CreateAsync(new CreateReservationDto
        {
            FlightId = "flight-1",
            PassengerName = "Ali",
            SeatNumber = "10B"
        }, "user-1", "Bearer token");

        var allReservations = await service.GetAllAsync();

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("AvailabilityUnavailable");
        availabilityClient.ConfirmCallCount.Should().Be(1);
        availabilityClient.ReleaseCallCount.Should().Be(1);
        allReservations.Should().BeEmpty();
    }
}