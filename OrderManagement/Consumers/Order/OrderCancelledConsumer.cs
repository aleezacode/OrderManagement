using System;
using Confluent.Kafka;
using MediatR;
using OrderManagement.Commands.Notification;
using OrderManagement.Models.Events;
using Microsoft.Extensions.Logging;

namespace OrderManagement.Consumers.Order
{
    public class OrderCancelledConsumer : BaseConsumer<OrderCancelled>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderCancelledConsumer> _logger;
        public OrderCancelledConsumer(IServiceProvider serviceProvider, ILogger<OrderCancelledConsumer> logger) : base(new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "notification-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            AutoCommitIntervalMs = 1000
        }, "order-cancelled", logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ProcessEventAsync(OrderCancelled @event, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var sendNotificationCommand = new SendNotificationCommand()
                {
                    OrderId = @event.OrderId,
                    Message = @event.Reason
                };

                _logger.LogInformation($"Sending cancellation SendNotification command for order: {@event.OrderId}");
                await mediator.Send(sendNotificationCommand, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing OrderCancelled event for order: {@event.OrderId}");
                throw;
            }
            
        }
    }
}