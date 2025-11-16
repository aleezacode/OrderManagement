using System;
using MediatR;
using OrderManagement.Commands.Notification;
using OrderManagement.Commands.Order.Cancellation;
using OrderManagement.Exceptions;
using OrderManagement.Kafka;
using OrderManagement.Models.Events;
using OrderManagement.Repositories;
using OrderModel = OrderManagement.Models.Order;

namespace OrderManagement.Handlers.Order.Cancellation
{
    public class CancelOrderBySystemHandler : IRequestHandler<CancelOrderBySystemCommand, bool>
    {
        private readonly IRepository<OrderModel> _orderRepository;
        private readonly IEventProducer _eventProducer;
        private readonly ILogger<CancelOrderBySystemHandler> _logger;

        public CancelOrderBySystemHandler(IRepository<OrderModel> orderRepository, IEventProducer eventProducer, ILogger<CancelOrderBySystemHandler> logger)
        {
            _orderRepository = orderRepository;
            _eventProducer = eventProducer;
            _logger = logger;
        }
        public async Task<bool> Handle(CancelOrderBySystemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Handling CancelOrderBySystem command for order {request.OrderId}");
                var order = await _orderRepository.GetByIdAsync(request.OrderId);

                order.OrderStatus = OrderStatus.Cancelled;
                await _orderRepository.UpdateAsync(request.OrderId, order);

                var orderCancelledEvent = new OrderCancelled()
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    Reason = request.Reason,
                    CancelledBy = "System"
                };

                _logger.LogInformation($"Publishing OrderCancelled event for order: {order.Id}");
                await _eventProducer.ProduceAsync("order-cancelled", orderCancelledEvent, cancellationToken);
                _logger.LogInformation($"Published OrderCancelled event for order: {order.Id}");

                return true;
            }
            catch (DocumentNotFoundException ex)
            {
                _logger.LogError(ex, $"Failed to find order with id {request.OrderId}");
                throw;
            }
            catch (DocumentUpdatedFailedException ex)
            {
                _logger.LogError(ex, $"Failed to update order with id {request.OrderId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while handling CancelOrderBySystem command for order: {request.OrderId}");
                throw;
            }
        }
    }
}