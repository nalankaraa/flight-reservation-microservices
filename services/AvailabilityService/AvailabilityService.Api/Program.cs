using AvailabilityService.Application.Repositories;
using AvailabilityService.Application.Services;
using AvailabilityService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<ISeatHoldRepository, InMemorySeatHoldRepository>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService.Application.Services.AvailabilityService>();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program { }