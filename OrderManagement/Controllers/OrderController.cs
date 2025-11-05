using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Commands.Order.Cancellation;
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
            var orderItems = new OrderItem
            {
                ProductId = request.Item.ProductId,
                Quantity = request.Item.Quantity,
                UnitPrice = request.Item.UnitPrice
            };

            var placeOrderCommand = new PlaceOrderCommand(request.UserId, orderItems);

            var orderId = await _mediator.Send(placeOrderCommand);
            return Ok(new { OrderId = orderId });
        }

        [HttpPut("cancel")]
        public async Task<IActionResult> CancelOrder([FromBody] CancelOrderRequest request)
        {
            var cancelOrderCommand = new CancelOrderByUserCommand(request.OrderId, request.Reason);

            var cancelled = await _mediator.Send(cancelOrderCommand);
            return Ok(cancelled);
        }
    }
}