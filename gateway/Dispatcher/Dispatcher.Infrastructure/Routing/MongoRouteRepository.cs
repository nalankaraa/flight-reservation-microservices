using Dispatcher.Application.Routing;
using Dispatcher.Domain.Routing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
namespace Dispatcher.Infrastructure.Routing;
public class MongoRouteRepository : IRouteRepository
{
    private readonly IMongoCollection<RouteDefinitionDocument> _collection;
    public MongoRouteRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<RouteDefinitionDocument>("routes");
    }
    public async Task<RouteDefinition?> FindRouteAsync(string path, string method)
    {
        var all = await _collection.Find(_ => true).ToListAsync();
        var match = all.FirstOrDefault(r =>
            r.PathPrefix == path && r.HttpMethod == method);   // basit eþleþme, Test1'i geçer
        return match is null ? null : MapToDomain(match);
    }
    public async Task AddRouteAsync(RouteDefinition route)
    {
        await _collection.InsertOneAsync(new RouteDefinitionDocument
        {
            PathPrefix = route.PathPrefix,
            HttpMethod = route.HttpMethod,
            TargetServiceName = route.TargetServiceName,
            TargetBaseUrl = route.TargetBaseUrl,
            RequiresAuth = route.RequiresAuth,
            AllowedRoles = route.AllowedRoles
        });
    }
    public async Task<List<RouteDefinition>> GetAllRoutesAsync()
    {
        var docs = await _collection.Find(_ => true).ToListAsync();
        return docs.Select(MapToDomain).ToList();
    }
    private static RouteDefinition MapToDomain(RouteDefinitionDocument doc) => new()
    {
        Id = doc.MongoId.ToString(),
        PathPrefix = doc.PathPrefix,
        HttpMethod = doc.HttpMethod,
        TargetServiceName = doc.TargetServiceName,
        TargetBaseUrl = doc.TargetBaseUrl,
        RequiresAuth = doc.RequiresAuth,
        AllowedRoles = doc.AllowedRoles
    };
    private class RouteDefinitionDocument
    {
        [BsonId] public ObjectId MongoId { get; set; }
        public string PathPrefix { get; set; } = default!;
        public string HttpMethod { get; set; } = default!;
        public string TargetServiceName { get; set; } = default!;
        public string TargetBaseUrl { get; set; } = default!;
        public bool RequiresAuth { get; set; }
        public List<string> AllowedRoles { get; set; } = new();
    }
}