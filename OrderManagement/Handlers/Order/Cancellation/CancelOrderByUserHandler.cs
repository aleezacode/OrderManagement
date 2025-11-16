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
using OrderManagement.Exceptions;

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

                if (order.OrderStatus != OrderStatus.Placed)
                {
                    _logger.LogWarning($"Order cannot be cancelled. Order: {order.Id} has status {order.OrderStatus}");
                    throw new InvalidOperationException($"Order has status {order.OrderStatus}");
                }

                _logger.LogInformation($"Releasing stock for product {order.Item.ProductId} from order: {order.Id}");
                var releaseStockCommand = new ReleaseStockCommand(order.Item.ProductId, order.Item.Quantity);
                await _mediator.Send(releaseStockCommand, cancellationToken);

                order.OrderStatus = OrderStatus.Cancelled;
                await _orderRepository.UpdateAsync(order.Id, order);

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
            catch (DocumentNotFoundException ex)
            {
                _logger.LogError(ex, $"Document not found while cancelling order {request.OrderId}");
                throw;
            }
            catch (DocumentUpdatedFailedException ex)
            {
                _logger.LogError(ex, $"Failed to update document while cancelling order {request.OrderId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while handling CancelOrderByUser for order: {request.OrderId}");
                throw;
            }
        }
    }
}