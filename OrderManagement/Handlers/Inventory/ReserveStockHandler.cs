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
using MongoDB.Driver;

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

        public async Task<string> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
        {
            var inventoryItems = new List<InventoryModel>();

            foreach (var item in request.ReservedItems)
            {
                var inventoryItem = await _inventoryRepository.GetByIdAsync(item.ProductId);
                if (inventoryItem == null || inventoryItem.Quantity < item.Quantity)
                {
                    var inventoryShortageEvent = new InventoryShortage
                    {
                        OrderId = request.OrderId,
                        ProductId = item.ProductId,
                        Message = "Insufficient stock"
                    };
                    await _eventProducer.ProduceAsync("inventory", inventoryShortageEvent, cancellationToken);
                    throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");
                }
                inventoryItems.Add(inventoryItem);
            }

            for (int i = 0; i < request.ReservedItems.Count; i++)
            {
                var item = request.ReservedItems[i];
                var inventoryItem = inventoryItems[i];
                inventoryItem.Quantity -= item.Quantity;
                await _inventoryRepository.UpdateAsync(inventoryItem.Id, inventoryItem);
            }

            var inventoryReservedEvent = new InventoryReserved
            {
                OrderId = request.OrderId,
                Reason = "Stock reserved successfully"
            };
            await _eventProducer.ProduceAsync("inventory", inventoryReservedEvent, cancellationToken);

            return request.OrderId;
       }
    }
}