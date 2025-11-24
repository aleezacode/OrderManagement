using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace OrderManagement.Commands.Payment
{
    public record ProcessPaymentCommand(string orderId) : IRequest<bool>;
}