using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace OrderManagement.Commands.Order
{
    public record CancelOrderCommand(string OrderId, string Reason) : IRequest;
}