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
using OrderManagement.Exceptions;

namespace OrderManagement.Handlers.Inventory
{
    public class ReserveStockHandler : IRequestHandler<ReserveStockCommand, bool>
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

        public async Task<bool> Handle(ReserveStockCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Handling ReserveStock command for order: {command.OrderId}, product: {command.ReservedItem.ProductId}, quantity: {command.ReservedItem.Quantity}");
                var inventoryItem = await _inventoryRepository.FindOneAsync(inv => inv.ProductId == command.ReservedItem.ProductId);

                if (inventoryItem.Quantity < command.ReservedItem.Quantity)
                {
                    var inventoryShortageEvent = new InventoryShortage
                    {
                        OrderId = command.OrderId,
                        ProductId = inventoryItem?.ProductId ?? command.ReservedItem.ProductId,
                        RequestedQuantity = command.ReservedItem.Quantity,
                        AvailableQuantity = inventoryItem?.Quantity ?? 0,
                        Message = "Insufficient stock"
                    };

                    await _eventProducer.ProduceAsync("inventory-shortage", inventoryShortageEvent, cancellationToken);
                    throw new InsufficientStockException(command.ReservedItem.ProductId, command.ReservedItem.Quantity, inventoryItem.Quantity);
                }

                inventoryItem.Quantity -= command.ReservedItem.Quantity;
                await _inventoryRepository.UpdateAsync(inventoryItem.Id, inventoryItem);

                var inventoryReservedEvent = new InventoryReserved
                {
                    OrderId = command.OrderId,
                    ProductId = inventoryItem.ProductId.ToString(),
                    Reason = "Stock reserved successfully"
                };

                _logger.LogInformation($"Publishing InventoryReserved event for order: {command.OrderId}");
                await _eventProducer.ProduceAsync("inventory-reserve", inventoryReservedEvent, cancellationToken);
                _logger.LogInformation($"Published InventoryReserved event for order {command.OrderId}");

                return true;
            }
            catch (DocumentNotFoundException ex)
            {
                _logger.LogError(ex, $"Failed to find inventory for productId {command.ReservedItem.ProductId}");
                throw;
            }
            catch (DocumentUpdatedFailedException ex)
            {
                _logger.LogError(ex, $"Failed to update inventory for productId {command.ReservedItem.ProductId}");
                throw;
            }
            catch (InsufficientStockException ex)
            {
                _logger.LogError(ex,$"Failed to reserve stock for productId {command.ReservedItem.ProductId}. Stock insufficient");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error while ReserveStock command for order: {command.OrderId}");
                throw;
            }
       }
    }
}