using Dispatcher.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<SecurityMiddleware>();

app.MapControllers();

app.Run();

public partial class Program { }