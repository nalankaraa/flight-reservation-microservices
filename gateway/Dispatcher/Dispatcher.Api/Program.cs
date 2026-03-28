using Dispatcher.Application.Forwarding;
using Dispatcher.Application.Logging;
using Dispatcher.Application.Routing;
using Dispatcher.Api.Middleware;
using Dispatcher.Domain.Routing;
using Dispatcher.Infrastructure.Http;
using Dispatcher.Infrastructure.Logging;
using Dispatcher.Infrastructure.Routing;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IRequestLogRepository, InMemoryRequestLogRepository>();
builder.Services.AddHttpClient<IRequestForwarder, HttpRequestForwarder>();

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient("mongodb://localhost:27017"));

builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("dispatcher-db");
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
app.UseMiddleware<SecurityMiddleware>();

app.MapControllers();

app.Run();

public partial class Program { }