using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AuthService.Application.Dtos;
using Dispatcher.Application.Forwarding;

namespace Dispatcher.Tests;

public class ScenarioRequestForwarder : IRequestForwarder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Dictionary<string, ScenarioUser> _usersByEmail = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin@system.local"] = new("admin-1", "admin@system.local", "Admin123!", "Admin"),
        ["customer@system.local"] = new("customer-1", "customer@system.local", "Customer123!", "Customer")
    };

    private readonly Dictionary<string, ScenarioFlight> _flights = new();
    private readonly Dictionary<string, ScenarioReservation> _reservations = new();
    private readonly Dictionary<string, ScenarioPayment> _payments = new();
    private readonly Dictionary<string, ScenarioNotification> _notifications = new();
    private readonly Dictionary<string, ScenarioSeatState> _seatStates = new();

    public async Task<HttpResponseMessage> ForwardAsync(
        string method,
        string targetUrl,
        Dictionary<string, string> headers,
        Stream body)
    {
        var uri = new Uri(targetUrl);
        var path = uri.AbsolutePath;
        var jsonBody = await ReadBodyAsync(body);

        return (method.ToUpperInvariant(), path) switch
        {
            ("POST", "/api/auth/login") => HandleLogin(jsonBody),
            ("POST", "/api/flights") => HandleCreateFlight(jsonBody),
            ("GET", var p) when p.StartsWith("/api/availability/", StringComparison.OrdinalIgnoreCase) => HandleAvailability(path),
            ("POST", "/api/reservations") => HandleCreateReservation(jsonBody, headers),
            ("GET", var p) when p.StartsWith("/api/payments/", StringComparison.OrdinalIgnoreCase) => HandleGetPayment(path),
            ("PATCH", var p) when p.StartsWith("/api/payments/", StringComparison.OrdinalIgnoreCase) => HandleUpdatePayment(path, jsonBody),
            ("GET", var p) when p.StartsWith("/api/notifications/user/", StringComparison.OrdinalIgnoreCase) => HandleGetNotifications(path),
            _ => JsonResponse(HttpStatusCode.NotFound, new { message = $"Unhandled route: {method} {path}" })
        };
    }

    private HttpResponseMessage HandleLogin(string jsonBody)
    {
        var request = JsonSerializer.Deserialize<LoginRequestDto>(jsonBody, JsonOptions);

        if (request is null ||
            !_usersByEmail.TryGetValue(request.Email, out var user) ||
            user.Password != request.Password)
        {
            return JsonResponse(HttpStatusCode.Unauthorized, new AuthResponseDto
            {
                Success = false,
                Message = "Invalid credentials."
            });
        }

        var token = JwtTestTokenFactory.CreateToken(user.Role, user.Id, user.Email);

        return JsonResponse(HttpStatusCode.OK, new AuthResponseDto
        {
            Success = true,
            Token = token,
            Message = "Login successful.",
            User = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                CreatedAtUtc = DateTime.UtcNow
            }
        });
    }

    private HttpResponseMessage HandleCreateFlight(string jsonBody)
    {
        var request = JsonSerializer.Deserialize<FlightCreateRequest>(jsonBody, JsonOptions);

        if (request is null)
        {
            return JsonResponse(HttpStatusCode.BadRequest, new { message = "Invalid flight payload." });
        }

        var id = $"flight-{_flights.Count + 1}";
        var flight = new ScenarioFlight(
            id,
            request.From,
            request.To,
            request.DepartureTime,
            request.ArrivalTime,
            request.Price,
            request.AvailableSeatCount);

        _flights[id] = flight;

        return JsonResponse(HttpStatusCode.Created, new
        {
            id = flight.Id,
            from = flight.From,
            to = flight.To,
            departureTime = flight.DepartureTime,
            arrivalTime = flight.ArrivalTime,
            price = flight.Price,
            availableSeatCount = flight.AvailableSeatCount
        });
    }

    private HttpResponseMessage HandleAvailability(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 3)
        {
            return JsonResponse(HttpStatusCode.NotFound, new { message = "Flight not found." });
        }

        var flightId = segments[2];

        if (!_flights.TryGetValue(flightId, out var flight))
        {
            return JsonResponse(HttpStatusCode.NotFound, new { message = "Flight not found." });
        }

        var occupiedSeats = _seatStates.Values.Count(x => x.FlightId == flightId && x.Status == "Reserved");

        return JsonResponse(HttpStatusCode.OK, new
        {
            flightId,
            totalSeats = flight.AvailableSeatCount,
            totalTrackedSeats = _seatStates.Values.Count(x => x.FlightId == flightId),
            availableSeats = flight.AvailableSeatCount - occupiedSeats,
            lockedSeats = occupiedSeats
        });
    }

    private HttpResponseMessage HandleCreateReservation(string jsonBody, Dictionary<string, string> headers)
    {
        var user = GetUserFromHeaders(headers);
        var request = JsonSerializer.Deserialize<ReservationCreateRequest>(jsonBody, JsonOptions);

        if (user is null || request is null)
        {
            return JsonResponse(HttpStatusCode.BadRequest, new { message = "Invalid reservation payload." });
        }

        if (!_flights.TryGetValue(request.FlightId, out var flight))
        {
            return JsonResponse(HttpStatusCode.NotFound, new
            {
                success = false,
                errorCode = "FlightNotFound",
                message = "The selected flight could not be found."
            });
        }

        var normalizedSeatNumber = request.SeatNumber.Trim().ToUpperInvariant();
        var seatKey = $"{request.FlightId}::{normalizedSeatNumber}";

        if (_seatStates.TryGetValue(seatKey, out var existingSeat) && existingSeat.Status == "Reserved")
        {
            return JsonResponse(HttpStatusCode.Conflict, new
            {
                success = false,
                errorCode = "SeatConflict",
                flightId = request.FlightId,
                seatNumber = normalizedSeatNumber,
                message = "The selected seat is already reserved."
            });
        }

        var reservationId = $"reservation-{_reservations.Count + 1}";
        var paymentId = $"payment-{_payments.Count + 1}";

        _seatStates[seatKey] = new ScenarioSeatState(request.FlightId, normalizedSeatNumber, user.Id, "Reserved");
        _reservations[reservationId] = new ScenarioReservation(reservationId, user.Id, request.FlightId, request.PassengerName, normalizedSeatNumber, paymentId);
        _payments[paymentId] = new ScenarioPayment(paymentId, reservationId, user.Id, flight.Price, "Pending");

        return JsonResponse(HttpStatusCode.Created, new
        {
            success = true,
            id = reservationId,
            userId = user.Id,
            flightId = request.FlightId,
            passengerName = request.PassengerName,
            seatNumber = normalizedSeatNumber,
            paymentId,
            paymentStatus = "Pending",
            message = "Reservation created successfully."
        });
    }

    private HttpResponseMessage HandleGetPayment(string path)
    {
        var paymentId = path.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();

        if (!_payments.TryGetValue(paymentId, out var payment))
        {
            return JsonResponse(HttpStatusCode.NotFound, new { message = "Payment not found." });
        }

        return JsonResponse(HttpStatusCode.OK, new
        {
            id = payment.Id,
            reservationId = payment.ReservationId,
            userId = payment.UserId,
            amount = payment.Amount,
            status = payment.Status,
            createdAtUtc = DateTime.UtcNow
        });
    }

    private HttpResponseMessage HandleUpdatePayment(string path, string jsonBody)
    {
        var paymentId = path.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
        var request = JsonSerializer.Deserialize<PaymentStatusUpdateRequest>(jsonBody, JsonOptions);

        if (!_payments.TryGetValue(paymentId, out var payment) || request is null)
        {
            return JsonResponse(HttpStatusCode.BadRequest, new { message = "Payment status cannot be updated." });
        }

        payment.Status = request.Status.Trim();

        if (payment.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            var notificationId = $"notification-{_notifications.Count + 1}";
            _notifications[notificationId] = new ScenarioNotification(
                notificationId,
                payment.UserId,
                "Payment Completed",
                $"Payment for reservation {payment.ReservationId} has been completed.",
                "PaymentCompleted");
        }

        return new HttpResponseMessage(HttpStatusCode.NoContent);
    }

    private HttpResponseMessage HandleGetNotifications(string path)
    {
        var userId = path.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
        var notifications = _notifications.Values
            .Where(x => x.UserId == userId)
            .Select(x => new
            {
                id = x.Id,
                userId = x.UserId,
                title = x.Title,
                message = x.Message,
                type = x.Type,
                createdAtUtc = DateTime.UtcNow,
                isRead = false,
                isSent = false
            })
            .ToList();

        return JsonResponse(HttpStatusCode.OK, notifications);
    }

    private static ScenarioUser? GetUserFromHeaders(Dictionary<string, string> headers)
    {
        if (!headers.TryGetValue("Authorization", out var authorizationHeader) ||
            !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = authorizationHeader["Bearer ".Length..];
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        var userId = jwt.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
        var email = jwt.Claims.First(x => x.Type == ClaimTypes.Email).Value;
        var role = jwt.Claims.First(x => x.Type == ClaimTypes.Role).Value;

        return new ScenarioUser(userId, email, string.Empty, role);
    }

    private static async Task<string> ReadBodyAsync(Stream body)
    {
        if (body == Stream.Null)
            return string.Empty;

        if (body.CanSeek)
            body.Position = 0;

        using var reader = new StreamReader(body, Encoding.UTF8, leaveOpen: true);
        var content = await reader.ReadToEndAsync();

        if (body.CanSeek)
            body.Position = 0;

        return content;
    }

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, object payload)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8,
                "application/json")
        };
    }

    private sealed record ScenarioUser(string Id, string Email, string Password, string Role);
    private sealed record ScenarioFlight(string Id, string From, string To, DateTime DepartureTime, DateTime ArrivalTime, decimal Price, int AvailableSeatCount);
    private sealed record ScenarioReservation(string Id, string UserId, string FlightId, string PassengerName, string SeatNumber, string PaymentId);
    private sealed class ScenarioPayment
    {
        public ScenarioPayment(string id, string reservationId, string userId, decimal amount, string status)
        {
            Id = id;
            ReservationId = reservationId;
            UserId = userId;
            Amount = amount;
            Status = status;
        }

        public string Id { get; }
        public string ReservationId { get; }
        public string UserId { get; }
        public decimal Amount { get; }
        public string Status { get; set; }
    }
    private sealed record ScenarioNotification(string Id, string UserId, string Title, string Message, string Type);
    private sealed record ScenarioSeatState(string FlightId, string SeatNumber, string UserId, string Status);
    private sealed class FlightCreateRequest
    {
        public string From { get; set; } = default!;
        public string To { get; set; } = default!;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeatCount { get; set; }
    }
    private sealed class ReservationCreateRequest
    {
        public string FlightId { get; set; } = default!;
        public string PassengerName { get; set; } = default!;
        public string SeatNumber { get; set; } = default!;
    }
    private sealed class PaymentStatusUpdateRequest
    {
        public string Status { get; set; } = default!;
    }
}