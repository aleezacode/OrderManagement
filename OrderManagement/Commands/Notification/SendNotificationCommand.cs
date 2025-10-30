using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderManagement.Commands.Notification
{
    public class SendNotificationCommand
    {
        public string OrderId { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
    }
}