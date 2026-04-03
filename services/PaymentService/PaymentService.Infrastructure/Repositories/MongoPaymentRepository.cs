using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PaymentService.Application.Repositories;
using PaymentService.Domain.Entities;

namespace PaymentService.Infrastructure.Repositories;

public class MongoPaymentRepository : IPaymentRepository
{
    private readonly IMongoCollection<PaymentDocument> _collection;

    public MongoPaymentRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<PaymentDocument>("payments");
    }

    public async Task AddAsync(Payment payment)
    {
        await _collection.InsertOneAsync(MapToDocument(payment));
    }

    public async Task<Payment?> GetByIdAsync(string id)
    {
        var document = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return document is null ? null : MapToDomain(document);
    }

    public async Task UpdateAsync(Payment payment)
    {
        await _collection.ReplaceOneAsync(x => x.Id == payment.Id, MapToDocument(payment));
    }

    private static PaymentDocument MapToDocument(Payment payment)
    {
        return new PaymentDocument
        {
            Id = payment.Id,
            ReservationId = payment.ReservationId,
            UserId = payment.UserId,
            Amount = payment.Amount,
            Status = payment.Status,
            CreatedAtUtc = payment.CreatedAtUtc
        };
    }

    private static Payment MapToDomain(PaymentDocument document)
    {
        return new Payment
        {
            Id = document.Id,
            ReservationId = document.ReservationId,
            UserId = document.UserId,
            Amount = document.Amount,
            Status = document.Status,
            CreatedAtUtc = document.CreatedAtUtc
        };
    }

    private class PaymentDocument
    {
        [BsonId]
        public string Id { get; set; } = default!;
        public string ReservationId { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; }
    }
}