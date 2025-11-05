using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace OrderManagement.Commands.Inventory
{
    public record ReleaseStockCommand(string ProductId, int ReleaseQuantity) : IRequest<bool>;
}