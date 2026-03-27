using Dispatcher.Application.Forwarding;
using Dispatcher.Application.Logging;
using Dispatcher.Api.Middleware;
using Dispatcher.Domain.Routing;
using Dispatcher.Infrastructure.Http;
using Dispatcher.Infrastructure.Logging;
using Dispatcher.Infrastructure.Routing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IRouteResolver, InMemoryRouteResolver>();
builder.Services.AddSingleton<IRequestLogRepository, InMemoryRequestLogRepository>();
builder.Services.AddHttpClient<IRequestForwarder, HttpRequestForwarder>();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<SecurityMiddleware>();

app.MapControllers();

app.Run();

public partial class Program { }