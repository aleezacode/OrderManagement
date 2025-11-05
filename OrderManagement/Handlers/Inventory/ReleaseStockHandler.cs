using System;
using MediatR;
using OrderManagement.Commands.Inventory;
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

                if (inventory == null)
                {
                    _logger.LogError($"Inventory for product: {request.ProductId} does not exist");
                    throw new Exception();
                }

                var previousQuantity = inventory.Quantity;
                inventory.Quantity += request.ReleaseQuantity;
                var updated = await _inventoryRepository.UpdateAsync(inventory.Id, inventory);

                if (!updated)
                {
                    _logger.LogError($"Failed to update inventory for product: {inventory.ProductId}");
                    throw new Exception();
                }

                var stockReleasedEvent = new StockReleased()
                {
                    ProductId = inventory.ProductId,
                    ReleasedQuantity = request.ReleaseQuantity,
                    PreviousQuantity = previousQuantity,
                    NewQuantity = inventory.Quantity,
                    Reason = "User cancelled order"
                };

                _logger.LogInformation($"Publishing StockReleased event for product: {inventory.ProductId}");
                await _eventProducer.ProduceAsync("stock-released", stockReleasedEvent);
                _logger.LogInformation($"Published StockReleased event for product: {inventory.ProductId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to handle ReleaseStockCommand for product {request.ProductId}");
                throw;
            }
        }
    }
}