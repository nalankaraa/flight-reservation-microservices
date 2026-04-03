using Dispatcher.Application.Logging;
using Dispatcher.Domain.Logging;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Dispatcher.Infrastructure.Logging;

public class MongoRequestLogRepository : IRequestLogRepository
{
    private readonly IMongoCollection<RequestLogDocument> _collection;

    public MongoRequestLogRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<RequestLogDocument>("requestLogs");

        var timestampIndex = Builders<RequestLogDocument>.IndexKeys.Descending(x => x.TimestampUtc);
        _collection.Indexes.CreateOne(new CreateIndexModel<RequestLogDocument>(timestampIndex));
    }

    public async Task AddAsync(RequestLog log)
    {
        var document = MapToDocument(log);

        if (string.IsNullOrWhiteSpace(document.Id))
        {
            document.Id = Guid.NewGuid().ToString();
        }

        await _collection.InsertOneAsync(document);
    }

    public async Task<List<RequestLog>> GetRecentAsync(int count = 100)
    {
        var documents = await _collection.Find(FilterDefinition<RequestLogDocument>.Empty)
            .SortByDescending(x => x.TimestampUtc)
            .Limit(count)
            .ToListAsync();

        return documents.Select(MapToDomain).ToList();
    }

    private static RequestLogDocument MapToDocument(RequestLog log)
    {
        return new RequestLogDocument
        {
            Id = log.Id,
            TimestampUtc = log.TimestampUtc,
            Path = log.Path,
            Method = log.Method,
            StatusCode = log.StatusCode,
            DurationMs = log.DurationMs,
            TargetService = log.TargetService,
            ErrorMessage = log.ErrorMessage
        };
    }

    private static RequestLog MapToDomain(RequestLogDocument document)
    {
        return new RequestLog
        {
            Id = document.Id,
            TimestampUtc = document.TimestampUtc,
            Path = document.Path,
            Method = document.Method,
            StatusCode = document.StatusCode,
            DurationMs = document.DurationMs,
            TargetService = document.TargetService,
            ErrorMessage = document.ErrorMessage
        };
    }

    private sealed class RequestLogDocument
    {
        [BsonId]
        public string Id { get; set; } = default!;
        public DateTime TimestampUtc { get; set; }
        public string Path { get; set; } = default!;
        public string Method { get; set; } = default!;
        public int StatusCode { get; set; }
        public double DurationMs { get; set; }
        public string? TargetService { get; set; }
        public string? ErrorMessage { get; set; }
    }
}