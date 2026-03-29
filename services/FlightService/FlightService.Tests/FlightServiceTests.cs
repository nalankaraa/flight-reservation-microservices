using FlightService.Application.Dtos;
using FluentAssertions;
using Xunit;

namespace FlightService.Tests;

public class FlightServiceTests
{
    [Fact]
    public async Task CreateAsync_Should_Create_Flight()
    {
        // Arrange
        var repository = new FakeFlightRepository();
        var service = new FlightService.Application.Services.FlightService(repository);

        var request = new CreateFlightDto
        {
            From = "IST",
            To = "ANK",
            DepartureTime = DateTime.UtcNow.AddDays(1),
            ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Price = 1500,
            AvailableSeatCount = 50
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.From.Should().Be("IST");
        result.To.Should().Be("ANK");
        repository.Flights.Should().ContainSingle();
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Flight_When_Id_Exists()
    {
        // Arrange
        var repository = new FakeFlightRepository();
        var service = new FlightService.Application.Services.FlightService(repository);

        var created = await service.CreateAsync(new CreateFlightDto
        {
            From = "IST",
            To = "ANK",
            DepartureTime = DateTime.UtcNow.AddDays(1),
            ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Price = 1500,
            AvailableSeatCount = 50
        });

        // Act
        var result = await service.GetByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Flights()
    {
        // Arrange
        var repository = new FakeFlightRepository();
        var service = new FlightService.Application.Services.FlightService(repository);

        await service.CreateAsync(new CreateFlightDto
        {
            From = "IST",
            To = "ANK",
            DepartureTime = DateTime.UtcNow.AddDays(1),
            ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(1),
            Price = 1500,
            AvailableSeatCount = 50
        });

        await service.CreateAsync(new CreateFlightDto
        {
            From = "ANK",
            To = "IZM",
            DepartureTime = DateTime.UtcNow.AddDays(2),
            ArrivalTime = DateTime.UtcNow.AddDays(2).AddHours(1),
            Price = 1800,
            AvailableSeatCount = 40
        });

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }
}