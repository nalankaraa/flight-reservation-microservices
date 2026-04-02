using AvailabilityService.Application.Repositories;
using AvailabilityService.Application.Services;
using AvailabilityService.Infrastructure.Repositories;
using BuildingBlocks.Infrastructure.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSharedJwtAuthentication(builder.Configuration);

builder.Services.AddSingleton<ISeatHoldRepository, InMemorySeatHoldRepository>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService.Application.Services.AvailabilityService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
