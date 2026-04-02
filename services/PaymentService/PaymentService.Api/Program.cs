using PaymentService.Application.Repositories;
using PaymentService.Application.Services;
using PaymentService.Infrastructure.Repositories;
using BuildingBlocks.Infrastructure.Auth;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var mongoSettings = builder.Configuration.GetSection("Mongo");
var mongoConnectionString = mongoSettings["ConnectionString"] ?? throw new InvalidOperationException("Mongo:ConnectionString is required.");
var mongoDatabaseName = mongoSettings["DatabaseName"] ?? throw new InvalidOperationException("Mongo:DatabaseName is required.");

builder.Services.AddControllers();
builder.Services.AddSharedJwtAuthentication(builder.Configuration);
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

builder.Services.AddScoped<IPaymentRepository, MongoPaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService.Application.Services.PaymentService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
