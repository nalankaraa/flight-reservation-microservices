using FluentAssertions;
using ReservationService.Application.Dtos;
using Xunit;

namespace ReservationService.Tests;

public class ReservationServiceTests
{
	[Fact]
	public async Task CreateReservation_Should_Return_Reservation()
	{
		// Arrange
		var repository = new FakeReservationRepository();
		var service = new ReservationService.Application.Services.ReservationService(repository);

		var request = new CreateReservationDto
		{
			FlightId = "flight-1",
			PassengerName = "Beyza",
			SeatNumber = "12A"
		};

		// Act
		var result = await service.CreateAsync(request);

		// Assert
		result.Should().NotBeNull();
		result.FlightId.Should().Be("flight-1");
		result.PassengerName.Should().Be("Beyza");
		result.SeatNumber.Should().Be("12A");
	}

	[Fact]
	public async Task GetReservations_Should_Return_List()
	{
		// Arrange
		var repository = new FakeReservationRepository();
		var service = new ReservationService.Application.Services.ReservationService(repository);

		await service.CreateAsync(new CreateReservationDto
		{
			FlightId = "flight-1",
			PassengerName = "Ali",
			SeatNumber = "10B"
		});

		// Act
		var result = await service.GetAllAsync();

		// Assert
		result.Should().HaveCount(1);
	}
}