using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using MediatR;
using OrderManagement.Models.Events.Orders;
using OrderManagement.Commands.Inventory;
using Microsoft.Extensions.Logging;

namespace OrderManagement.Consumers.Order
{
    public class OrderPlacedConsumer : BaseConsumer<OrderPlaced>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderPlacedConsumer> _logger;
        public OrderPlacedConsumer(IServiceProvider serviceProvider, ILogger<OrderPlacedConsumer> logger) : base(new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "inventory-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            AutoCommitIntervalMs = 1000
        }, "order-placed", logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ProcessEventAsync(OrderPlaced @event, CancellationToken cancellationToken)
        {
            try
            {
               using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var reservedItem = new ReservedItem
                {
                    ProductId = @event.Item.ProductId,
                    Quantity = @event.Item.Quantity
                };

                _logger.LogInformation($"Sending ReserveStock command for order: {@event.OrderId}");
                await mediator.Send(new ReserveStockCommand(@event.OrderId, reservedItem), cancellationToken); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing OrderPlaced event for order: {@event.OrderId}");
                throw;
            }
        }
    }
}