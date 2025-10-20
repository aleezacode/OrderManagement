using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OrderManagement.Models.Enums;

namespace OrderManagement.Models
{
    public class Payment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string OrderId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string UserId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }
}