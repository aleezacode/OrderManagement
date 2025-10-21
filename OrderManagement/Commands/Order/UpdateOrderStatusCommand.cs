using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace OrderManagement.Commands.Order
{
    public record UpdateOrderStatusCommand(string OrderId, OrderStatus OrderStatus) : IRequest;
}