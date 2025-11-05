using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace OrderManagement.Commands.Order.Cancellation
{
    public record CancelOrderByUserCommand(string OrderId, string Reason) : IRequest<bool>;
}