using System.Net.Http.Headers;
using ReservationService.Application.Clients;
using ReservationService.Application.Repositories;
using ReservationService.Application.Services;
using ReservationService.Infrastructure.Repositories;
using ReservationService.Infrastructure.Clients;
using BuildingBlocks.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);
var availabilityBaseUrl = builder.Configuration["Services:Availability:BaseUrl"] ?? "http://localhost:5099/";

builder.Services.AddControllers();
builder.Services.AddSharedJwtAuthentication(builder.Configuration);
builder.Services.AddHttpClient<ISeatAvailabilityClient, AvailabilityApiClient>(client =>
{
    client.BaseAddress = new Uri(availabilityBaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddScoped<IReservationService, ReservationService.Application.Services.ReservationService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }