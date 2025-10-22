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
        private readonly IMediator _mediator;
        public OrderPlacedConsumer(IMediator mediator) : base(new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "order-management-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        }, "orders")
        {
            _mediator = mediator;
        }

        protected override async Task ProcessEventAsync(OrderPlaced @event, CancellationToken cancellationToken)
        {
            var reservedItems = new List<ReservedItem>();
            foreach (var item in @event.Items)
            {
                reservedItems.Add(new ReservedItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });
            }
            await _mediator.Send(new ReserveStockCommand(@event.OrderId, reservedItems), cancellationToken);
        }
    }
}