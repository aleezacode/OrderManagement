using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using OrderManagement.Kafka;
using OrderManagement.Models.Events;
using OrderManagement.Repositories;
using InventoryModel = OrderManagement.Models.Inventory;
using OrderManagement.Commands.Inventory;
using OrderManagement.Models.Events.Inventory;

namespace OrderManagement.Handlers.Inventory
{
    public class ReserveStockHandler : IRequestHandler<ReserveStockCommand, string>
    {
        private readonly IRepository<InventoryModel> _inventoryRepository;
        private readonly IEventProducer _eventProducer;
        public ReserveStockHandler(IRepository<InventoryModel> inventoryRepository, IEventProducer eventProducer)
        {
            _inventoryRepository = inventoryRepository;
            _eventProducer = eventProducer;
        }

        public async Task<string> Handle(ReserveStockCommand command, CancellationToken cancellationToken)
        {
            var inventoryItem = await _inventoryRepository.FindOneAsync(inv => inv.ProductId == command.ReservedItem.ProductId);
            if (inventoryItem == null || inventoryItem.Quantity < command.ReservedItem.Quantity)
            {
                var inventoryShortageEvent = new InventoryShortage
                {
                    OrderId = command.OrderId,
                    ProductId = inventoryItem?.ProductId.ToString() ?? command.ReservedItem.ProductId,
                    RequestedQuantity = command.ReservedItem.Quantity,
                    AvailableQuantity = inventoryItem?.Quantity ?? 0,
                    Message = "Insufficient stock"
                };
                await _eventProducer.ProduceAsync("inventory", inventoryShortageEvent, cancellationToken);
                throw new InvalidOperationException($"Insufficient stock for product {command.ReservedItem.ProductId}");
            }
                
            inventoryItem.Quantity -= command.ReservedItem.Quantity;
            await _inventoryRepository.UpdateAsync(inventoryItem.Id, inventoryItem);

            var inventoryReservedEvent = new InventoryReserved
            {
                OrderId = command.OrderId,
                ProductId = inventoryItem.ProductId.ToString(),
                Reason = "Stock reserved successfully"
            };
            await _eventProducer.ProduceAsync("inventory", inventoryReservedEvent, cancellationToken);

            return command.OrderId;
       }
    }
}