using Dispatcher.Application.Routing;
using Dispatcher.Domain.Routing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Dispatcher.Infrastructure.Routing;

public class MongoRouteRepository : IRouteRepository
{
    private readonly IMongoCollection<RouteDefinitionDocument> _collection;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private volatile List<RouteDefinitionDocument>? _cachedRoutes;

    public MongoRouteRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<RouteDefinitionDocument>("routes");
    }

    public async Task<RouteDefinition?> FindRouteAsync(string path, string method)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(method))
            return null;

        var all = await GetCachedRoutesAsync();

        var normalizedPath = path.TrimEnd('/');

        var match = all
            .Where(r => r.HttpMethod.Equals(method, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.PathPrefix.Length)
            .FirstOrDefault(r =>
            {
                var prefix = r.PathPrefix.TrimEnd('/');

                return normalizedPath.Equals(prefix, StringComparison.OrdinalIgnoreCase)
                       || normalizedPath.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase);
            });

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
        InvalidateCache();
    }

    public async Task UpsertRouteAsync(RouteDefinition route)
    {
        var filter = Builders<RouteDefinitionDocument>.Filter.And(
            Builders<RouteDefinitionDocument>.Filter.Eq(x => x.PathPrefix, route.PathPrefix),
            Builders<RouteDefinitionDocument>.Filter.Eq(x => x.HttpMethod, route.HttpMethod));

        var update = Builders<RouteDefinitionDocument>.Update
            .Set(x => x.PathPrefix, route.PathPrefix)
            .Set(x => x.HttpMethod, route.HttpMethod)
            .Set(x => x.TargetServiceName, route.TargetServiceName)
            .Set(x => x.TargetBaseUrl, route.TargetBaseUrl)
            .Set(x => x.RequiresAuth, route.RequiresAuth)
            .Set(x => x.AllowedRoles, route.AllowedRoles);

        await _collection.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions
            {
                IsUpsert = true
            });

        InvalidateCache();
    }

    public async Task<List<RouteDefinition>> GetAllRoutesAsync()
    {
        var docs = await GetCachedRoutesAsync();
        return docs.Select(MapToDomain).ToList();
    }

    private async Task<List<RouteDefinitionDocument>> GetCachedRoutesAsync()
    {
        var snapshot = _cachedRoutes;
        if (snapshot is not null)
        {
            return snapshot;
        }

        await _cacheLock.WaitAsync();
        try
        {
            snapshot = _cachedRoutes;
            if (snapshot is not null)
            {
                return snapshot;
            }

            snapshot = await _collection.Find(_ => true).ToListAsync();
            _cachedRoutes = snapshot;
            return snapshot;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private void InvalidateCache()
    {
        _cachedRoutes = null;
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
