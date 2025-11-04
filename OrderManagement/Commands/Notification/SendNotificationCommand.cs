using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace OrderManagement.Commands.Notification
{
    public class SendNotificationCommand : IRequest<string>
    {
        public string OrderId { get; set; }
        public string Message { get; set; }
    }
}