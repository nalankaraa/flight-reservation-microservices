using FlightService.Application.Repositories;
using FlightService.Application.Services;
using FlightService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IFlightRepository, InMemoryFlightRepository>();
builder.Services.AddScoped<IFlightService, FlightService.Application.Services.FlightService>();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program { }