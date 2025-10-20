using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrderManagement.Models
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string UserId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public required string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}