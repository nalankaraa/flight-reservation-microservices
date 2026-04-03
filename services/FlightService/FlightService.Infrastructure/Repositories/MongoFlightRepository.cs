using FlightService.Application.Repositories;
using FlightService.Domain.Entities;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace FlightService.Infrastructure.Repositories;

public class MongoFlightRepository : IFlightRepository
{
    private readonly IMongoCollection<FlightDocument> _collection;

    public MongoFlightRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<FlightDocument>("flights");
    }

    public async Task AddAsync(Flight flight)
    {
        await _collection.InsertOneAsync(MapToDocument(flight));
    }

    public async Task<Flight?> GetByIdAsync(string id)
    {
        var document = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return document is null ? null : MapToDomain(document);
    }

    public async Task<List<Flight>> GetAllAsync()
    {
        var documents = await _collection.Find(FilterDefinition<FlightDocument>.Empty).ToListAsync();
        return documents.Select(MapToDomain).ToList();
    }

    public async Task UpdateAsync(Flight flight)
    {
        await _collection.ReplaceOneAsync(x => x.Id == flight.Id, MapToDocument(flight));
    }

    public async Task DeleteAsync(string id)
    {
        await _collection.DeleteOneAsync(x => x.Id == id);
    }

    private static FlightDocument MapToDocument(Flight flight)
    {
        return new FlightDocument
        {
            Id = flight.Id,
            From = flight.From,
            To = flight.To,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            Price = flight.Price,
            AvailableSeatCount = flight.AvailableSeatCount
        };
    }

    private static Flight MapToDomain(FlightDocument document)
    {
        return new Flight
        {
            Id = document.Id,
            From = document.From,
            To = document.To,
            DepartureTime = document.DepartureTime,
            ArrivalTime = document.ArrivalTime,
            Price = document.Price,
            AvailableSeatCount = document.AvailableSeatCount
        };
    }

    private class FlightDocument
    {
        [BsonId]
        public string Id { get; set; } = default!;
        public string From { get; set; } = default!;
        public string To { get; set; } = default!;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeatCount { get; set; }
    }
}
