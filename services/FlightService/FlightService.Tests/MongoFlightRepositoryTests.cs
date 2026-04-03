using FluentAssertions;
using FlightService.Domain.Entities;
using FlightService.Infrastructure.Repositories;

namespace FlightService.Tests;

public class MongoFlightRepositoryTests : IClassFixture<MongoFlightTestFixture>
{
    private readonly MongoFlightTestFixture _fixture;

    public MongoFlightRepositoryTests(MongoFlightTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_Should_Persist_Flight_And_Return_It_By_Id()
    {
        var database = _fixture.CreateDatabase();
        var repository = new MongoFlightRepository(database);
        var flight = CreateFlight("flight-1", "IST", "ANK");

        await repository.AddAsync(flight);

        var persisted = await repository.GetByIdAsync(flight.Id);

        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be("flight-1");
        persisted.From.Should().Be("IST");
        persisted.To.Should().Be("ANK");
    }

    [Fact]
    public async Task UpdateAsync_And_DeleteAsync_Should_Modify_Persisted_Flight()
    {
        var database = _fixture.CreateDatabase();
        var repository = new MongoFlightRepository(database);
        var flight = CreateFlight("flight-2", "IST", "ADB");

        await repository.AddAsync(flight);

        flight.To = "AYT";
        flight.Price = 2450;
        flight.AvailableSeatCount = 99;

        await repository.UpdateAsync(flight);

        var updated = await repository.GetByIdAsync(flight.Id);
        updated.Should().NotBeNull();
        updated!.To.Should().Be("AYT");
        updated.Price.Should().Be(2450);
        updated.AvailableSeatCount.Should().Be(99);

        await repository.DeleteAsync(flight.Id);

        var deleted = await repository.GetByIdAsync(flight.Id);
        var all = await repository.GetAllAsync();

        deleted.Should().BeNull();
        all.Should().BeEmpty();
    }

    private static Flight CreateFlight(string id, string from, string to)
    {
        return new Flight
        {
            Id = id,
            From = from,
            To = to,
            DepartureTime = DateTime.UtcNow.AddDays(1),
            ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Price = 1800,
            AvailableSeatCount = 50
        };
    }
}
