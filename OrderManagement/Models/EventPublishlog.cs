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
    public class EventPublishlog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public required string OrderId { get; set; }
        public string? EventType { get; set; }
        public string? EventMessage { get; set; }
        public required DateTime PublishedAt { get; set; }
    }
}