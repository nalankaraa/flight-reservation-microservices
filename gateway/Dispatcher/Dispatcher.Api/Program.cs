using Dispatcher.Application.Forwarding;
using Dispatcher.Application.Logging;
using Dispatcher.Application.Routing;
using Dispatcher.Api.Middleware;
using Dispatcher.Domain.Routing;
using Dispatcher.Infrastructure.Http;
using Dispatcher.Infrastructure.Logging;
using Dispatcher.Infrastructure.Routing;
using BuildingBlocks.Infrastructure.Auth;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var mongoSettings = builder.Configuration.GetSection("Mongo");
var mongoConnectionString = mongoSettings["ConnectionString"] ?? "mongodb://localhost:27017";
var mongoDatabaseName = mongoSettings["DatabaseName"] ?? "dispatcher-db";

builder.Services.AddControllers();
builder.Services.AddSharedJwtAuthentication(builder.Configuration);

builder.Services.AddSingleton<IRequestLogRepository, InMemoryRequestLogRepository>();
builder.Services.AddHttpClient<IRequestForwarder, HttpRequestForwarder>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

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

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<SecurityMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
