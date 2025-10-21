using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace OrderManagement.Commands.Order
{
    public record PlaceOrderCommand(string UserId, List<OrderItem> Items) : IRequest<string>;
    public record OrderItem
    {
        public string ProductId { get; init; }
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
    }
}