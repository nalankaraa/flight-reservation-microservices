using AvailabilityService.Domain.Entities;
using AvailabilityService.Infrastructure.Repositories;
using FluentAssertions;

namespace AvailabilityService.Tests;

public class MongoSeatHoldRepositoryTests : IClassFixture<MongoSeatHoldRepositoryTestFixture>
{
    private readonly MongoSeatHoldRepositoryTestFixture _fixture;

    public MongoSeatHoldRepositoryTests(MongoSeatHoldRepositoryTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TryLockSeatAsync_Then_GetByFlightAndSeatAsync_Should_Return_Persisted_Hold()
    {
        var repository = new MongoSeatHoldRepository(_fixture.CreateDatabase());
        var now = DateTime.UtcNow;
        var hold = new SeatHold
        {
            Id = "flight-1::1A",
            FlightId = "flight-1",
            SeatNumber = "1A",
            UserId = "user-1",
            ReservedUntilUtc = now.AddMinutes(10),
            Status = "Locked",
            LastUpdatedUtc = now
        };

        await repository.TryLockSeatAsync(hold, now);

        var result = await repository.GetByFlightAndSeatAsync("flight-1", "1a");

        result.Should().NotBeNull();
        result!.FlightId.Should().Be("flight-1");
        result.SeatNumber.Should().Be("1A");
        result.UserId.Should().Be("user-1");
        result.Status.Should().Be("Locked");
    }

    [Fact]
    public async Task ConfirmSeatAsync_Should_Update_Seat_Status_To_Reserved()
    {
        var repository = new MongoSeatHoldRepository(_fixture.CreateDatabase());
        var now = DateTime.UtcNow;
        var hold = new SeatHold
        {
            Id = "flight-2::2B",
            FlightId = "flight-2",
            SeatNumber = "2B",
            UserId = "user-2",
            ReservedUntilUtc = now.AddMinutes(10),
            Status = "Locked",
            LastUpdatedUtc = now
        };

        await repository.TryLockSeatAsync(hold, now);

        var confirmed = await repository.ConfirmSeatAsync("flight-2", "2b", "user-2", now.AddMinutes(1));

        confirmed.Should().NotBeNull();
        confirmed!.Status.Should().Be("Reserved");
        confirmed.UserId.Should().Be("user-2");
    }
}