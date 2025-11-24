using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Commands.Order.Cancellation;
using OrderManagement.Commands.Order;
using OrderManagement.Requests;
using OrderManagement.Commands.Payment;
using System.ComponentModel.DataAnnotations;

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
            return Ok(new { Success = cancelled });
        }

        [HttpPost("payment")]
        public async Task<IActionResult> ProcessPayment(
            [Required(ErrorMessage = "OrderId is required")]
            [RegularExpression(@"^[a-fA-F0-9]{24}$", ErrorMessage = "OrderId must be a valid ObjectId")]
            string orderId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var processPaymentCommand = new ProcessPaymentCommand(orderId);
            var result = await _mediator.Send(processPaymentCommand);
            return Ok(new {Success = result });
        }
    }
}