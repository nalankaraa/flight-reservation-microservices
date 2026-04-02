using Dispatcher.Application.Forwarding;
using Dispatcher.Api.Middleware;
using Dispatcher.Domain.Routing;
using Dispatcher.Infrastructure.Http;
using Dispatcher.Infrastructure.Routing;
<<<<<<< Updated upstream
=======
using BuildingBlocks.Infrastructure.Auth;
using MongoDB.Driver;
>>>>>>> Stashed changes

var builder = WebApplication.CreateBuilder(args);

var mongoSettings = builder.Configuration.GetSection("Mongo");
var mongoConnectionString = mongoSettings["ConnectionString"] ?? throw new InvalidOperationException("Mongo:ConnectionString is required.");
var mongoDatabaseName = mongoSettings["DatabaseName"] ?? throw new InvalidOperationException("Mongo:DatabaseName is required.");

builder.Services.AddControllers();
<<<<<<< Updated upstream
builder.Services.AddSingleton<IRouteResolver, InMemoryRouteResolver>();
builder.Services.AddHttpClient<IRequestForwarder, HttpRequestForwarder>();

var app = builder.Build();

=======
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IRequestLogRepository, InMemoryRequestLogRepository>();
builder.Services.AddHttpClient<IRequestForwarder, HttpRequestForwarder>();

builder.Services.AddSharedJwtAuthentication(builder.Configuration);

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(mongoConnectionString));

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

builder.Services.AddSingleton<IRouteRepository, MongoRouteRepository>();
builder.Services.AddSingleton<IRouteResolver, DatabaseRouteResolver>();
builder.Services.AddSingleton<RouteSeedService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<RouteSeedService>();
    await seedService.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Dispatcher API");
        options.SwaggerEndpoint("/swagger-proxy/auth/swagger.json", "AuthService API");
        options.SwaggerEndpoint("/swagger-proxy/availability/swagger.json", "AvailabilityService API");
        options.SwaggerEndpoint("/swagger-proxy/flight/swagger.json", "FlightService API");
        options.SwaggerEndpoint("/swagger-proxy/notification/swagger.json", "NotificationService API");
        options.SwaggerEndpoint("/swagger-proxy/payment/swagger.json", "PaymentService API");
        options.SwaggerEndpoint("/swagger-proxy/reservation/swagger.json", "ReservationService API");
    });
}

app.UseCors();
app.UseAuthentication();
app.UseMiddleware<RequestLoggingMiddleware>();
>>>>>>> Stashed changes
app.UseMiddleware<SecurityMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
