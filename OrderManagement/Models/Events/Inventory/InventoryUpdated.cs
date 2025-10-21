using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Events.Inventory
{
    public class InventoryUpdated
    {
        public string ProductId { get; set; }
        public int NewQuantity { get; set; }
        public int PreviousQuantity { get; set; }
        public int QuantityChange { get; set; }
        public string Reason { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}