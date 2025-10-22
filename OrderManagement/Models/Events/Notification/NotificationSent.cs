using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Models.Events.Notification
{
    public class NotificationSent : IEvent
    {
        public string NotificationId { get; set; }
        public string UserId { get; set; }
        public string Type { get; set; }
        public string Trigger { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}