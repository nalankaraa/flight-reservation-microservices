using ReservationService.Application.Repositories;
using ReservationService.Application.Services;
using ReservationService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddScoped<IReservationService, ReservationService.Application.Services.ReservationService>();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program { }