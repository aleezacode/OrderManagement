using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using MediatR;
using OrderManagement.Models.Events.Orders;
using OrderManagement.Commands.Inventory;

namespace OrderManagement.Consumers
{
    public class OrderPlacedConsumer : BaseConsumer<OrderPlaced>
    {
        private readonly IServiceProvider _serviceProvider;
        public OrderPlacedConsumer(IServiceProvider serviceProvider) : base(new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "order-management-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        }, "orders")
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ProcessEventAsync(OrderPlaced @event, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            
            var reservedItem = new ReservedItem
            {
                ProductId = @event.Item.ProductId,
                Quantity = @event.Item.Quantity
            };
            
            await mediator.Send(new ReserveStockCommand(@event.OrderNumber, reservedItem), cancellationToken);
        }
    }
}