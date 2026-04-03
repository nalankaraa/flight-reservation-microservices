using AvailabilityService.Application.Repositories;
using AvailabilityService.Application.Services;
using AvailabilityService.Application.Clients;
using AvailabilityService.Infrastructure.Clients;
using AvailabilityService.Infrastructure.Repositories;
using BuildingBlocks.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
var mongoSettings = builder.Configuration.GetSection("Mongo");
var mongoConnectionString = mongoSettings["ConnectionString"] ?? throw new InvalidOperationException("Mongo:ConnectionString is required.");
var mongoDatabaseName = mongoSettings["DatabaseName"] ?? throw new InvalidOperationException("Mongo:DatabaseName is required.");
var flightBaseUrl = builder.Configuration["Services:Flight:BaseUrl"] ?? "http://localhost:5092/";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Availability Service API",
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
builder.Services.AddHttpClient<IFlightCapacityClient, FlightApiClient>(client =>
{
    client.BaseAddress = new Uri(flightBaseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
builder.Services.AddSingleton(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

builder.Services.AddScoped<ISeatHoldRepository, MongoSeatHoldRepository>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService.Application.Services.AvailabilityService>();

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

public partial class Program { }
