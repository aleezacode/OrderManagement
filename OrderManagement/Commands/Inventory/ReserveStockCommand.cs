using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace OrderManagement.Commands.Inventory
{
    public record ReserveStockCommand(string OrderId, ReservedItem ReservedItem) : IRequest<string>;

    public record ReservedItem
    {
        public string ProductId { get; init; }
        public int Quantity { get; init; }
    }
}