using ReservationService.Application.Repositories;
using ReservationService.Application.Services;
using ReservationService.Infrastructure.Repositories;
using BuildingBlocks.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSharedJwtAuthentication(builder.Configuration);

builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddScoped<IReservationService, ReservationService.Application.Services.ReservationService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
