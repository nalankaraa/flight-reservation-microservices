using FluentAssertions;
using ReservationService.Application.Exceptions;
using ReservationService.Domain.Entities;
using ReservationService.Infrastructure.Repositories;

namespace ReservationService.Tests;

public class MongoReservationRepositoryTests : IClassFixture<MongoReservationTestFixture>
{
    private readonly MongoReservationTestFixture _fixture;

    public MongoReservationRepositoryTests(MongoReservationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_Should_Persist_Reservation()
    {
        var database = _fixture.CreateDatabase();
        var repository = new MongoReservationRepository(database);

        await repository.AddAsync(new Reservation
        {
            Id = "1",
            UserId = "user-1",
            FlightId = "flight-1",
            PassengerName = "Ali",
            SeatNumber = "10B"
        });

        var all = await repository.GetAllAsync();

        all.Should().HaveCount(1);
        all[0].SeatNumber.Should().Be("10B");
    }

    [Fact]
    public async Task AddAsync_Should_Throw_DuplicateSeatReservationException_For_Same_Flight_And_Seat()
    {
        var database = _fixture.CreateDatabase();
        var repository = new MongoReservationRepository(database);

        await repository.AddAsync(new Reservation
        {
            Id = "1",
            UserId = "user-1",
            FlightId = "flight-1",
            PassengerName = "Ali",
            SeatNumber = "10B"
        });

        var act = async () => await repository.AddAsync(new Reservation
        {
            Id = "2",
            UserId = "user-2",
            FlightId = "flight-1",
            PassengerName = "Ayse",
            SeatNumber = "10B"
        });

        await act.Should().ThrowAsync<DuplicateSeatReservationException>();
    }

    [Fact]
    public async Task AddAsync_Should_Treat_SeatNumber_As_CaseInsensitive_For_Duplicates()
    {
        var database = _fixture.CreateDatabase();
        var repository = new MongoReservationRepository(database);

        await repository.AddAsync(new Reservation
        {
            Id = "1",
            UserId = "user-1",
            FlightId = "flight-1",
            PassengerName = "Ali",
            SeatNumber = "10b"
        });

        var act = async () => await repository.AddAsync(new Reservation
        {
            Id = "2",
            UserId = "user-2",
            FlightId = "flight-1",
            PassengerName = "Ayse",
            SeatNumber = "10B"
        });

        await act.Should().ThrowAsync<DuplicateSeatReservationException>();
    }
}
