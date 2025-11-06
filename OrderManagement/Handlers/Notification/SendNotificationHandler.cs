using System;
using MediatR;
using OrderManagement.Commands.Notification;
using OrderManagement.Repositories;
using OrderModel = OrderManagement.Models.Order;
using NotificationModel = OrderManagement.Models.Notification;
using OrderManagement.Kafka;
using OrderManagement.Models.Events.Notification;
using OrderManagement.Models;
using OrderManagement.Exceptions;

namespace OrderManagement.Handlers.Notification
{
    public class SendNotificationHandler : IRequestHandler<SendNotificationCommand, string>
    {
        private readonly IRepository<OrderModel> _orderRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IEventProducer _eventProducer;
        private readonly ILogger<SendNotificationHandler> _logger;
        public SendNotificationHandler(IRepository<OrderModel> orderRepository, IRepository<NotificationModel> notificationRepository, IRepository<User> userRepository, IEventProducer eventProducer, ILogger<SendNotificationHandler> logger)
        {
            _orderRepository = orderRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _eventProducer = eventProducer;
            _logger = logger;
        }
        public async Task<string> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Handling SendNotificationCommand for order: {request.OrderId}");
                var order = await _orderRepository.GetByIdAsync(request.OrderId);
                var user = await _userRepository.GetByIdAsync(order.UserId);

                var notification = new NotificationModel()
                {
                    UserId = user.Id,
                    OrderId = order.Id,
                    Type = user.NotificationType,
                    //TODO: Make the Message from the original message to an enum and then map it
                    Message = request.Message,
                    Status = NotificationStatus.Sent
                };

                await _notificationRepository.CreateAsync(notification);

                var notificationSent = new NotificationSent()
                {
                    NotificationId = notification.Id,
                    UserId = user.Id,
                    OrderId = order.Id,
                    Type = user.NotificationType.ToString(),
                    Message = notification.Message,
                    Status = notification.Status.ToString()
                };

                _logger.LogInformation($"Publishing NotificationSent event for order: {order.Id}");
                await _eventProducer.ProduceAsync("notification-sent", notificationSent);
                _logger.LogInformation($"Published NotificationSent event for order: {order.Id}");

                return notification.Id!;
            }
            catch (DocumentNotFoundException ex)
            {
                _logger.LogError(ex, "Document not found in database. Look at the inner exception for more details");
                throw;
            }
            catch (DocumentCreationFailedException ex)
            {
                _logger.LogError(ex, $"Failed to create notification for order {request.OrderId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while SendNotification command for order: {request.OrderId}");
                throw;
            }
        }
    }
}