using System;
using MediatR;
using OrderManagement.Commands.Notification;
using OrderManagement.Repositories;
using OrderModel = OrderManagement.Models.Order;
using NotificationModel = OrderManagement.Models.Notification;
using OrderManagement.Kafka;
using OrderManagement.Models.Events.Notification;
using OrderManagement.Models;

namespace OrderManagement.Handlers.Notification
{
    public class SendNotificationHandler : IRequestHandler<SendNotificationCommand, string>
    {
        private readonly IRepository<OrderModel> _orderRepository;
        private readonly IRepository<NotificationModel> _notificationRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IEventProducer _eventProducer;
        public SendNotificationHandler(IRepository<OrderModel> orderRepository, IRepository<NotificationModel> notificationRepository, IRepository<User> userRepository, IEventProducer eventProducer)
        {
            _orderRepository = orderRepository;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _eventProducer = eventProducer;
        }
        public async Task<string> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            var user = await _userRepository.GetByIdAsync(order.UserId);
            //TODO: Improve error handling
            if (order == null) throw new Exception();
            if (user == null) throw new Exception();

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

            await _eventProducer.ProduceAsync("notification", notificationSent);
            return notification.Id!;
        }
    }
}