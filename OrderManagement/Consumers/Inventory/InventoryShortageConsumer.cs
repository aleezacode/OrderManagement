using System;
using Confluent.Kafka;
using MediatR;
using OrderManagement.Commands.Notification;
using OrderManagement.Commands.Order.Cancellation;
using OrderManagement.Models.Events.Inventory;
using Microsoft.Extensions.Logging;

namespace OrderManagement.Consumers.Inventory
{
    public class InventoryShortageConsumer : BaseConsumer<InventoryShortage>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InventoryShortageConsumer> _logger;
        public InventoryShortageConsumer(IServiceProvider serviceProvider, ILogger<InventoryShortageConsumer> logger) : base(new ConsumerConfig()
        {
            BootstrapServers = "kafka:9092",
            GroupId = "order-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            AutoCommitIntervalMs = 1000
        }, "inventory-shortage", logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ProcessEventAsync(InventoryShortage @event, CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var cancelOrderCommand = new CancelOrderBySystemCommand(@event.OrderId, @event.Message);

                _logger.LogInformation($"Sending CancelOrderBySystem command for order: {@event.OrderId}" );
                await mediator.Send(cancelOrderCommand, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex ,$"Error processing InventoryShortage event for order: {@event.OrderId}");
                throw;
            }
            
        }
    }
}