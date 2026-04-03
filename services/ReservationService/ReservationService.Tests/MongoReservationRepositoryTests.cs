using FluentAssertions;
using Mongo2Go;
using MongoDB.Driver;
using ReservationService.Application.Exceptions;
using ReservationService.Domain.Entities;
using ReservationService.Infrastructure.Repositories;

namespace ReservationService.Tests;

public class MongoReservationRepositoryTests : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly IMongoDatabase _database;

    public MongoReservationRepositoryTests()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("reservation-test-db");
    }

    [Fact]
    public async Task AddAsync_Should_Persist_Reservation()
    {
        var repository = new MongoReservationRepository(_database);

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
        var repository = new MongoReservationRepository(_database);

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

    public void Dispose()
    {
        _runner.Dispose();
    }
}
