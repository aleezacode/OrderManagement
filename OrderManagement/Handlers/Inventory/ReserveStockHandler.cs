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
        private readonly ILogger<ReserveStockHandler> _logger;
        public ReserveStockHandler(IRepository<InventoryModel> inventoryRepository, IEventProducer eventProducer, ILogger<ReserveStockHandler> logger)
        {
            _inventoryRepository = inventoryRepository;
            _eventProducer = eventProducer;
            _logger = logger;
        }

        public async Task<string> Handle(ReserveStockCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Handling ReserveStock command for order: {command.OrderId}, product: {command.ReservedItem.ProductId}, quantity: {command.ReservedItem.Quantity}");
                var inventoryItem = await _inventoryRepository.FindOneAsync(inv => inv.ProductId == command.ReservedItem.ProductId);
                if (inventoryItem == null || inventoryItem.Quantity < command.ReservedItem.Quantity)
                {
                    _logger.LogError($"No inventory found for product: {command.ReservedItem.ProductId}");
                    var inventoryShortageEvent = new InventoryShortage
                    {
                        OrderId = command.OrderId,
                        ProductId = inventoryItem?.ProductId ?? command.ReservedItem.ProductId,
                        RequestedQuantity = command.ReservedItem.Quantity,
                        AvailableQuantity = inventoryItem?.Quantity ?? 0,
                        Message = "Insufficient stock"
                    };
                    await _eventProducer.ProduceAsync("inventory-shortage", inventoryShortageEvent, cancellationToken);
                    _logger.LogError($"Stock insufficient for product: {inventoryItem.ProductId}");
                    throw new InvalidOperationException($"Insufficient stock for product {command.ReservedItem.ProductId}");
                }
                    
                inventoryItem.Quantity -= command.ReservedItem.Quantity;
                var updated = await _inventoryRepository.UpdateAsync(inventoryItem.Id, inventoryItem);

                if (!updated)
                {
                    _logger.LogError($"Failed to update stock for product: {inventoryItem.ProductId}");
                    throw new Exception();
                }

                var inventoryReservedEvent = new InventoryReserved
                {
                    OrderId = command.OrderId,
                    ProductId = inventoryItem.ProductId.ToString(),
                    Reason = "Stock reserved successfully"
                };

                _logger.LogInformation($"Publishing InventoryReserved event for order: {command.OrderId}");
                await _eventProducer.ProduceAsync("inventory-reserve", inventoryReservedEvent, cancellationToken);
                _logger.LogInformation($"Published InventoryReserved event for order {command.OrderId}");

                return command.OrderId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,$"Failed to handle ReserveStock command for order: {command.OrderId}");
                throw;
            }
       }
    }
}