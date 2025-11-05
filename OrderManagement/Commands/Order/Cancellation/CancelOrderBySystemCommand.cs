using System;
using MediatR;

namespace OrderManagement.Commands.Order.Cancellation
{
    public record CancelOrderBySystemCommand(string OrderId, string Reason) : IRequest<bool>;
}