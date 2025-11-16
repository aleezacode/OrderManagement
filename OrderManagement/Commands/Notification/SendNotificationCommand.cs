using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace OrderManagement.Commands.Notification
{
    public record SendNotificationCommand(string OrderId, string Message) : IRequest<bool>;
}