using NotificationService.Application.Repositories;
using NotificationService.Application.Services;
using NotificationService.Infrastructure.Repositories;
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<INotificationRepository, MongoNotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService.Application.Services.NotificationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
