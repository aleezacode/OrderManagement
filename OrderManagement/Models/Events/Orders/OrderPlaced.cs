using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Events
{
    public class OrderPlaced
    {
        public string OrderId { get; set; }

        public string UserId { get; set; }

        public List<OrderItem> Items { get; set; } = new();

        public decimal TotalAmount { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class OrderItem
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}