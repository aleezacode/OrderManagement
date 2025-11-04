using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using MediatR;
using OrderManagement.Commands.Notification;
using OrderManagement.Models.Events.Inventory;

namespace OrderManagement.Consumers.Inventory
{
    public class InventoryReservedConsumer : BaseConsumer<InventoryReserved>
    {
        private readonly IServiceProvider _serviceProvider;
        public InventoryReservedConsumer(IServiceProvider serviceProvider) : base(new ConsumerConfig
        {
            BootstrapServers = "kafka:9092",
            GroupId = "inventory-service",
            AutoOffsetReset = AutoOffsetReset.Earliest
        }, "inventory")
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ProcessEventAsync(InventoryReserved @event, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var notificationCommand = new SendNotificationCommand
            {
                OrderId = @event.OrderId,
                Message = @event.Reason
            };
            
            await mediator.Send(notificationCommand, cancellationToken);
        }
    }
}