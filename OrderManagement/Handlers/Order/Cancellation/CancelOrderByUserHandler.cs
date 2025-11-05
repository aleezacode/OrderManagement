using System;
using MediatR;
using OrderManagement.Repositories;
using OrderModel = OrderManagement.Models.Order;
using OrderManagement.Commands.Order.Cancellation;
using OrderManagement.Commands.Inventory;
using OrderManagement.Commands.Notification;
using OrderManagement.Kafka;
using OrderManagement.Models.Events;
using OrderManagement.Models;

namespace OrderManagement.Handlers.Order.Cancellation
{
    public class CancelOrderByUserHandler : IRequestHandler<CancelOrderByUserCommand, bool>
    {
        private readonly IRepository<OrderModel> _orderRepository;
        private IMediator _mediator;
        private readonly IEventProducer _eventProducer;
        private ILogger<CancelOrderByUserHandler> _logger;
        public CancelOrderByUserHandler(IRepository<OrderModel> orderRepository, IMediator mediator, IEventProducer eventProducer, ILogger<CancelOrderByUserHandler> logger)
        {
            _orderRepository = orderRepository;
            _mediator = mediator;
            _eventProducer = eventProducer;
            _logger = logger;
        }

        public async Task<bool> Handle(CancelOrderByUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Handling CancelOrderByUser command for order: {request.OrderId}");

                var order = await _orderRepository.GetByIdAsync(request.OrderId);
                if (order == null)
                {
                    _logger.LogError($"Order with id: {request.OrderId} does not exist");
                    throw new Exception();
                }

                if (order.OrderStatus != OrderStatus.Placed)
                {
                    _logger.LogWarning($"Order cannot be cancelled. Order: {order.Id} has status {order.OrderStatus}");
                    //TODO: Add proper error handling
                    throw new InvalidOperationException("Order is already cancelled/failed/paid");
                }

                if (order.OrderStatus == OrderStatus.Placed)
                {
                    _logger.LogInformation($"Releasing stock for product {order.Item.ProductId} from order: {order.Id}");
                    var releaseStockCommand = new ReleaseStockCommand(order.Item.ProductId, order.Item.Quantity);
                    await _mediator.Send(releaseStockCommand, cancellationToken);
                }

                order.OrderStatus = OrderStatus.Cancelled;
                var updated = await _orderRepository.UpdateAsync(order.Id, order);

                if (!updated)
                {
                    _logger.LogError($"Failed to update orderStatus for order: {order.Id}");
                    throw new Exception();   
                }

                var orderCancelledEvent = new OrderCancelled()
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    Reason = request.Reason,
                    CancelledBy = "User"
                };

                _logger.LogInformation($"Publishing OrderCancelled event for order: {order.Id}");
                await _eventProducer.ProduceAsync("order-cancelled", orderCancelledEvent, cancellationToken);
                _logger.LogInformation($"Successfully published OrderCancelled event for order: {order.Id}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Failed to cancel order by user, order: {request.OrderId}");
                throw;
            }
        }
    }
}