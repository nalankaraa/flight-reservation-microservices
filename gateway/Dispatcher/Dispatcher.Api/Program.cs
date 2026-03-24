using Dispatcher.Application.Forwarding;
using Dispatcher.Api.Middleware;
using Dispatcher.Domain.Routing;
using Dispatcher.Infrastructure.Http;
using Dispatcher.Infrastructure.Routing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IRouteResolver, InMemoryRouteResolver>();
builder.Services.AddHttpClient<IRequestForwarder, HttpRequestForwarder>();

var app = builder.Build();

app.UseMiddleware<SecurityMiddleware>();

app.MapControllers();

app.Run();

public partial class Program { }