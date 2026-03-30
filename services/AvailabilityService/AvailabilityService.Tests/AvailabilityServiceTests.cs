using AvailabilityService.Application.Dtos;
using FluentAssertions;
using Xunit;

namespace AvailabilityService.Tests;

public class AvailabilityServiceTests
{
    [Fact]
    public async Task CreateHoldAsync_Should_Create_SeatHold()
    {
        // Arrange
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        var request = new CreateSeatHoldDto
        {
            FlightId = "flight-1",
            UserId = "user-1",
            SeatCount = 2,
            HoldMinutes = 10
        };

        // Act
        var result = await service.CreateHoldAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.FlightId.Should().Be("flight-1");
        result.UserId.Should().Be("user-1");
        result.SeatCount.Should().Be(2);
        result.Status.Should().Be("Pending");
        repository.Holds.Should().ContainSingle();
    }

    [Fact]
    public async Task GetHoldByIdAsync_Should_Return_Hold_When_It_Exists()
    {
        // Arrange
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        var created = await service.CreateHoldAsync(new CreateSeatHoldDto
        {
            FlightId = "flight-1",
            UserId = "user-1",
            SeatCount = 1,
            HoldMinutes = 10
        });

        // Act
        var result = await service.GetHoldByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetHoldByIdAsync_Should_Return_Null_When_Hold_Does_Not_Exist()
    {
        // Arrange
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        // Act
        var result = await service.GetHoldByIdAsync("missing-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ConfirmHoldAsync_Should_Set_Status_To_Confirmed()
    {
        // Arrange
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        var created = await service.CreateHoldAsync(new CreateSeatHoldDto
        {
            FlightId = "flight-1",
            UserId = "user-1",
            SeatCount = 1,
            HoldMinutes = 10
        });

        // Act
        var success = await service.ConfirmHoldAsync(created.Id);
        var updated = await service.GetHoldByIdAsync(created.Id);

        // Assert
        success.Should().BeTrue();
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task CancelHoldAsync_Should_Set_Status_To_Cancelled()
    {
        // Arrange
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        var created = await service.CreateHoldAsync(new CreateSeatHoldDto
        {
            FlightId = "flight-1",
            UserId = "user-1",
            SeatCount = 1,
            HoldMinutes = 10
        });

        // Act
        var success = await service.CancelHoldAsync(created.Id);
        var updated = await service.GetHoldByIdAsync(created.Id);

        // Assert
        success.Should().BeTrue();
        updated.Should().NotBeNull();
        updated!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task GetHoldByIdAsync_Should_Mark_Hold_As_Expired_When_Time_Has_Passed()
    {
        // Arrange
        var repository = new FakeSeatHoldRepository();
        var service = new AvailabilityService.Application.Services.AvailabilityService(repository);

        var result = await service.CreateHoldAsync(new CreateSeatHoldDto
        {
            FlightId = "flight-1",
            UserId = "user-1",
            SeatCount = 1,
            HoldMinutes = 1
        });

        var hold = await repository.GetByIdAsync(result.Id);
        hold!.ReservedUntilUtc = DateTime.UtcNow.AddMinutes(-1);
        await repository.UpdateAsync(hold);

        // Act
        var expired = await service.GetHoldByIdAsync(result.Id);

        // Assert
        expired.Should().NotBeNull();
        expired!.Status.Should().Be("Expired");
    }
}