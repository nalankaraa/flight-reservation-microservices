using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Api.Controllers;
using PaymentService.Application.Dtos;
using PaymentService.Application.Services;

namespace PaymentService.Tests;

public class PaymentsControllerOwnershipTests
{
    [Fact]
    public async Task GetById_Should_Return_Forbid_When_Customer_Requests_Another_Users_Payment()
    {
        var service = new StubPaymentService();
        service.Payments["payment-1"] = CreatePayment("payment-1", "user-2");
        var controller = CreateController(service, "Customer", "user-1");

        var result = await controller.GetById("payment-1");

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetById_Should_Return_Ok_When_Customer_Requests_Own_Payment()
    {
        var service = new StubPaymentService();
        service.Payments["payment-1"] = CreatePayment("payment-1", "user-1");
        var controller = CreateController(service, "Customer", "user-1");

        var result = await controller.GetById("payment-1");

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeAssignableTo<PaymentResponseDto>()
            .Which.UserId.Should().Be("user-1");
    }

    [Fact]
    public async Task UpdateStatus_Should_Return_Forbid_When_Customer_Updates_Another_Users_Payment()
    {
        var service = new StubPaymentService();
        service.Payments["payment-1"] = CreatePayment("payment-1", "user-2");
        var controller = CreateController(service, "Customer", "user-1");

        var result = await controller.UpdateStatus("payment-1", new UpdatePaymentStatusDto
        {
            Status = "Completed"
        });

        result.Should().BeOfType<ForbidResult>();
        service.CompletedPaymentIds.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateStatus_Should_Allow_Admin_To_Update_Any_Payment()
    {
        var service = new StubPaymentService();
        service.Payments["payment-1"] = CreatePayment("payment-1", "user-2");
        var controller = CreateController(service, "Admin", "admin-1");

        var result = await controller.UpdateStatus("payment-1", new UpdatePaymentStatusDto
        {
            Status = "Completed"
        });

        result.Should().BeOfType<NoContentResult>();
        service.CompletedPaymentIds.Should().ContainSingle("payment-1");
    }

    private static PaymentsController CreateController(IPaymentService paymentService, string role, string userId)
    {
        var controller = new PaymentsController(paymentService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Role, role)
                    ], "TestAuth"))
                }
            }
        };

        controller.ControllerContext.HttpContext.Request.Headers.Authorization = "Bearer token";
        return controller;
    }

    private static PaymentResponseDto CreatePayment(string id, string userId)
    {
        return new PaymentResponseDto
        {
            Id = id,
            ReservationId = "reservation-1",
            UserId = userId,
            Amount = 2500,
            Status = "Pending",
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    private sealed class StubPaymentService : IPaymentService
    {
        public Dictionary<string, PaymentResponseDto> Payments { get; } = new();
        public List<string> CompletedPaymentIds { get; } = [];

        public Task<PaymentResponseDto> CreateAsync(CreatePaymentDto request)
        {
            throw new NotSupportedException();
        }

        public Task<PaymentResponseDto?> GetByIdAsync(string id)
        {
            Payments.TryGetValue(id, out var payment);
            return Task.FromResult(payment);
        }

        public Task<bool> CompleteAsync(string id, string authorizationHeader)
        {
            CompletedPaymentIds.Add(id);
            return Task.FromResult(true);
        }

        public Task<bool> FailAsync(string id, string authorizationHeader)
        {
            throw new NotSupportedException();
        }
    }
}
