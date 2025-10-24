using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Events.Inventory
{
    public class InventoryReserved : IEvent
    {
        public Guid OrderNumber { get; set; }
        public string ProductId { get; set; }
        public string Reason { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}