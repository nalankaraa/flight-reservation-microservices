using AvailabilityService.Application.Dtos;
using FluentAssertions;
using Xunit;

namespace AvailabilityService.Tests;

public class AvailabilityServiceTests
{
    [Fact]
    public async Task LockSeatAsync_Should_Create_Seat_Lock()
    {
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        var result = await service.LockSeatAsync("flight-1", new LockSeatRequestDto
        {
            SeatNumber = "1a",
            HoldMinutes = 10
        }, "user-1");

        result.Should().NotBeNull();
        result!.FlightId.Should().Be("flight-1");
        result.SeatNumber.Should().Be("1A");
        result.UserId.Should().Be("user-1");
        result.Status.Should().Be("Locked");
        result.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task LockSeatAsync_Should_Return_Null_When_Seat_Is_Already_Locked()
    {
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        await service.LockSeatAsync("flight-1", new LockSeatRequestDto
        {
            SeatNumber = "2B",
            HoldMinutes = 10
        }, "user-1");

        var secondAttempt = await service.LockSeatAsync("flight-1", new LockSeatRequestDto
        {
            SeatNumber = "2B",
            HoldMinutes = 10
        }, "user-2");

        secondAttempt.Should().BeNull();
    }

    [Fact]
    public async Task GetSeatsAsync_Should_Mark_Expired_Lock_As_Available()
    {
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        await repository.TryLockSeatAsync(new Domain.Entities.SeatHold
        {
            Id = "flight-1::3C",
            FlightId = "flight-1",
            SeatNumber = "3C",
            UserId = "user-1",
            ReservedUntilUtc = DateTime.UtcNow.AddMinutes(-1),
            Status = "Locked",
            LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-2)
        }, DateTime.UtcNow.AddMinutes(-2));

        var seats = await service.GetSeatsAsync("flight-1");

        seats.Should().ContainSingle();
        seats[0].SeatNumber.Should().Be("3C");
        seats[0].IsAvailable.Should().BeTrue();
        seats[0].Status.Should().Be("Available");
        repository.Holds[0].Status.Should().Be("Expired");
    }

    [Fact]
    public async Task ReleaseSeatAsync_Should_Release_Owned_Lock()
    {
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        await service.LockSeatAsync("flight-1", new LockSeatRequestDto
        {
            SeatNumber = "4D",
            HoldMinutes = 10
        }, "user-1");

        var released = await service.ReleaseSeatAsync("flight-1", new ReleaseSeatRequestDto
        {
            SeatNumber = "4D"
        }, "user-1", allowAnyUser: false);

        var seats = await service.GetSeatsAsync("flight-1");

        released.Should().BeTrue();
        seats.Should().ContainSingle();
        seats[0].IsAvailable.Should().BeTrue();
        seats[0].Status.Should().Be("Available");
    }

    [Fact]
    public async Task ReleaseSeatAsync_Should_Fail_For_Other_User()
    {
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        await service.LockSeatAsync("flight-1", new LockSeatRequestDto
        {
            SeatNumber = "5E",
            HoldMinutes = 10
        }, "user-1");

        var released = await service.ReleaseSeatAsync("flight-1", new ReleaseSeatRequestDto
        {
            SeatNumber = "5E"
        }, "user-2", allowAnyUser: false);

        released.Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmSeatAsync_Should_Mark_Seat_As_Reserved()
    {
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        await service.LockSeatAsync("flight-1", new LockSeatRequestDto
        {
            SeatNumber = "6F",
            HoldMinutes = 10
        }, "user-1");

        var confirmed = await service.ConfirmSeatAsync("flight-1", "6f", "user-1");
        var seats = await service.GetSeatsAsync("flight-1");

        confirmed.Should().NotBeNull();
        confirmed!.Status.Should().Be("Reserved");
        confirmed.IsAvailable.Should().BeFalse();
        seats[0].Status.Should().Be("Reserved");
        seats[0].IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task GetAvailabilityAsync_Should_Return_Flight_Summary()
    {
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        await service.LockSeatAsync("flight-1", new LockSeatRequestDto
        {
            SeatNumber = "1A",
            HoldMinutes = 10
        }, "user-1");

        await repository.TryLockSeatAsync(new Domain.Entities.SeatHold
        {
            Id = "flight-1::1B",
            FlightId = "flight-1",
            SeatNumber = "1B",
            UserId = "user-2",
            ReservedUntilUtc = DateTime.UtcNow.AddMinutes(-1),
            Status = "Locked",
            LastUpdatedUtc = DateTime.UtcNow.AddMinutes(-2)
        }, DateTime.UtcNow.AddMinutes(-2));

        var summary = await service.GetAvailabilityAsync("flight-1");

        summary.FlightId.Should().Be("flight-1");
        summary.TotalTrackedSeats.Should().Be(2);
        summary.LockedSeats.Should().Be(1);
        summary.AvailableSeats.Should().Be(1);
    }
}