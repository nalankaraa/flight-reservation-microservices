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
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(method))
            return null;

        var all = await _collection.Find(_ => true).ToListAsync();

        var match = all.FirstOrDefault(r =>
            r.PathPrefix.Equals(path, StringComparison.OrdinalIgnoreCase) &&
            r.HttpMethod.Equals(method, StringComparison.OrdinalIgnoreCase));

        return match is null ? null : MapToDomain(match);
    }

    public async Task AddRouteAsync(RouteDefinition route)
    {
        var document = new RouteDefinitionDocument
        {
            PathPrefix = route.PathPrefix,
            HttpMethod = route.HttpMethod,
            TargetServiceName = route.TargetServiceName,
            TargetBaseUrl = route.TargetBaseUrl,
            RequiresAuth = route.RequiresAuth,
            AllowedRoles = route.AllowedRoles
        };

        await _collection.InsertOneAsync(document);
    }

    public async Task<List<RouteDefinition>> GetAllRoutesAsync()
    {
        var docs = await _collection.Find(_ => true).ToListAsync();
        return docs.Select(MapToDomain).ToList();
    }

    private static RouteDefinition MapToDomain(RouteDefinitionDocument doc)
    {
        return new RouteDefinition
        {
            Id = doc.MongoId.ToString(),
            PathPrefix = doc.PathPrefix,
            HttpMethod = doc.HttpMethod,
            TargetServiceName = doc.TargetServiceName,
            TargetBaseUrl = doc.TargetBaseUrl,
            RequiresAuth = doc.RequiresAuth,
            AllowedRoles = doc.AllowedRoles
        };
    }

    private class RouteDefinitionDocument
    {
        [BsonId]
        public ObjectId MongoId { get; set; }

        public string PathPrefix { get; set; } = default!;
        public string HttpMethod { get; set; } = default!;
        public string TargetServiceName { get; set; } = default!;
        public string TargetBaseUrl { get; set; } = default!;
        public bool RequiresAuth { get; set; }
        public List<string> AllowedRoles { get; set; } = new();
    }
}