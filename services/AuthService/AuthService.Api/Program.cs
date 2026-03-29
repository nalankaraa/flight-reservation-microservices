using AuthService.Application.Repositories;
using AuthService.Application.Services;
using AuthService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// InMemory kullanıyoruz şimdilik
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<ITokenService, SimpleTokenService>();
builder.Services.AddScoped<IAuthService, AuthService.Application.Services.AuthService>();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program { }