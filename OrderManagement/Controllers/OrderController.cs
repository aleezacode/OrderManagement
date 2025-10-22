using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Commands.Order;
using OrderManagement.Requests;

namespace OrderManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("api/orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var orderItems = request.items.Select(i => new OrderItem
            {
                ProductId = i.productId,
                Quantity = i.quantity,
                UnitPrice = i.unitPrice
            }).ToList();

            var placeOrderCommand = new PlaceOrderCommand(request.userId, orderItems);

            var orderId = await _mediator.Send(placeOrderCommand);
            return Ok(new { OrderId = orderId });
        }
    }
}