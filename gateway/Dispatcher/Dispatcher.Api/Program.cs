using Dispatcher.Application.Forwarding;
using Dispatcher.Application.Logging;
using Dispatcher.Application.Routing;
using Dispatcher.Api.Configuration;
using Dispatcher.Api.Middleware;
using Dispatcher.Domain.Routing;
using Dispatcher.Infrastructure.Http;
using Dispatcher.Infrastructure.Logging;
using Dispatcher.Infrastructure.Routing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var mongoSettings = builder.Configuration.GetSection("Mongo");
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = new JwtOptions
{
    Key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key is required."),
    Issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is required."),
    Audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is required.")
};
var mongoConnectionString = mongoSettings["ConnectionString"] ?? "mongodb://localhost:27017";
var mongoDatabaseName = mongoSettings["DatabaseName"] ?? "dispatcher-db";

builder.Services.AddControllers();
builder.Services.Configure<JwtOptions>(jwtSection);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

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
