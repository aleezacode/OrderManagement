using System;

namespace OrderManagement.Models.Events.Inventory
{
    public class StockReleased : IEvent
    {
        public string ProductId { get; set; }
        public int ReleasedQuantity { get; set; }
        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }
        public string Reason { get; set; }
        public DateTime TimeStamp = DateTime.UtcNow;
    }
}