using System;
using MediatR;
using OrderManagement.Commands.Inventory;
using OrderManagement.Exceptions;
using OrderManagement.Kafka;
using OrderManagement.Models.Events.Inventory;
using OrderManagement.Repositories;
using InventoryModel = OrderManagement.Models.Inventory;

namespace OrderManagement.Handlers.Inventory
{
    public class ReleaseStockHandler : IRequestHandler<ReleaseStockCommand, bool>
    {
        private readonly IRepository<InventoryModel> _inventoryRepository;
        private readonly IEventProducer _eventProducer;
        private readonly ILogger<ReleaseStockHandler> _logger;

        public ReleaseStockHandler(IRepository<InventoryModel> inventoryRepository, IEventProducer eventProducer, ILogger<ReleaseStockHandler> logger)
        {
            _inventoryRepository = inventoryRepository;
            _eventProducer = eventProducer;
            _logger = logger;
        }
        public async Task<bool> Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Handling ReleaseStock command for product: {request.ProductId}");
                var inventory = await _inventoryRepository.FindOneAsync(i => i.ProductId == request.ProductId);

                var previousQuantity = inventory.Quantity;
                inventory.Quantity += request.ReleaseQuantity;

                await _inventoryRepository.UpdateAsync(inventory.Id, inventory);

                return true;
            }
            catch (DocumentNotFoundException ex)
            {
                _logger.LogError(ex, $"Cannot release stock. Inventory for productId {request.ProductId} was not found");
                throw;
            }
            catch (DocumentUpdatedFailedException ex)
            {
                _logger.LogError(ex, $"Failed to update inventory for productId {request.ProductId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error when handling ReleaseStockCommand for product {request.ProductId}");
                throw;
            }
        }
    }
}