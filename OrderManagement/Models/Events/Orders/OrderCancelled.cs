using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Events
{
    public class OrderCancelled : IEvent
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public string Reason { get; set; }
        public string CancelledBy { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}