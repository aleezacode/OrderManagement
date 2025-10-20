using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Events.Payments
{
    public class PaymentFailed
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = "Failed";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}