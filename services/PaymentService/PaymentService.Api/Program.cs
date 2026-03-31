using PaymentService.Application.Repositories;
using PaymentService.Application.Services;
using PaymentService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService.Application.Services.PaymentService>();

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program { }