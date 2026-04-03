using Dispatcher.Application.Forwarding;
using Dispatcher.Application.Logging;
using Dispatcher.Application.Routing;
using Dispatcher.Api.Middleware;
using Dispatcher.Domain.Routing;
using Dispatcher.Infrastructure.Http;
using Dispatcher.Infrastructure.Logging;
using Dispatcher.Infrastructure.Routing;
using BuildingBlocks.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var mongoSettings = builder.Configuration.GetSection("Mongo");
var mongoConnectionString = mongoSettings["ConnectionString"] ?? "mongodb://localhost:27017";
var mongoDatabaseName = mongoSettings["DatabaseName"] ?? "dispatcher-db";

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // Dispatcher is a transparent gateway; downstream services remain responsible for payload validation.
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Dispatcher API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<SecurityMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
