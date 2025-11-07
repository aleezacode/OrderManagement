using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using MediatR;
using OrderManagement.Commands.Notification;
using OrderManagement.Models.Events.Inventory;
using Microsoft.Extensions.Logging;

namespace OrderManagement.Consumers.Inventory
{
    public class InventoryReservedConsumer : BaseConsumer<InventoryReserved>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InventoryReservedConsumer> _logger;
        public InventoryReservedConsumer(IServiceProvider serviceProvider, ILogger<InventoryReservedConsumer> logger) : base(new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "notification-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            AutoCommitIntervalMs = 1000
        }, "inventory-reserve", logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ProcessEventAsync(InventoryReserved @event, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var notificationCommand = new SendNotificationCommand(@event.OrderId, @event.Reason);

                _logger.LogInformation($"Sending reservedStock SendNotification command for order: {@event.OrderId}");
                await mediator.Send(notificationCommand, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing InventoryReserved for order: {@event.OrderId}");
                throw;
            }

        }
    }
}