using System;
using Moq;
using OrderManagement.Commands.Inventory;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Models.Events.Inventory;
using OrderManagement.Repositories;

namespace OrderManagementTests.IntegrationTests
{
    [Trait("Category", "Integration")]
    public class ReserveStockHandlerTest : BaseIntegrationTest
    {
        private IRepository<Inventory> _inventoryRepository;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _inventoryRepository = GetService<IRepository<Inventory>>();
        }

        [Fact]
        public async Task HandleReserveStockOK()
        {
            var order = await CreateTestOrder();
            var inventory = await _inventoryRepository.FindOneAsync(x => x.ProductId == order.Item.ProductId);

            var item = new ReservedItem
            {
                ProductId = order.Item.ProductId,
                Quantity = order.Item.Quantity
            };
            var command = new ReserveStockCommand(order.Id, item);
            var result = await Mediator.Send(command);
            Assert.True(result);

            var updatedInventory = await _inventoryRepository.FindOneAsync(x => x.ProductId == order.Item.ProductId);
            Assert.True(inventory.Quantity - item.Quantity == updatedInventory.Quantity);

            MockEventProducer.Verify(x => x.ProduceAsync(
                "inventory-reserve",
                It.IsAny<InventoryReserved>(),
    It.IsAny<CancellationToken>()),
            Times.Once);
        }

        [Fact]
        public async Task HandleReserveStockNotOK_InventoryInsufficient()
        {
            var order = await CreateTestOrder();
            var inventory = await _inventoryRepository.FindOneAsync(x => x.ProductId == order.Item.ProductId);
            inventory.Quantity = 0;
            await _inventoryRepository.UpdateAsync(inventory.Id, inventory);

            var item = new ReservedItem
            {
                ProductId = order.Item.ProductId,
                Quantity = order.Item.Quantity
            };
            var command = new ReserveStockCommand(order.Id, item);

            var exception = await Assert.ThrowsAsync<InsufficientStockException>(
                 () => Mediator.Send(command));

            Assert.Equal(order.Item.ProductId, exception.ProductId);
            Assert.Equal(0, exception.AvailableQuantity);
            Assert.Equal(item.Quantity, exception.RequestedQuantity);

            var unchangedInventory = await _inventoryRepository.FindOneAsync(x => x.ProductId == order.Item.ProductId);
            Assert.Equal(inventory.Quantity, unchangedInventory.Quantity);
            
            MockEventProducer.Verify(x => x.ProduceAsync(
            "inventory-shortage",
            It.IsAny<InventoryShortage>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

            inventory.Quantity = 30;
            await _inventoryRepository.UpdateAsync(inventory.Id, inventory);
        }
        
        [Fact]
        public async Task HandleReserveStockNotOK_EventPublishFailed()
        {
            var order = await CreateTestOrder();
            var inventory = await _inventoryRepository.FindOneAsync(x => x.ProductId == order.Item.ProductId);

            var item = new ReservedItem
            {
                ProductId = order.Item.ProductId,
                Quantity = order.Item.Quantity
            };

            MockEventProducer.Setup(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Event publish failed"));

            var command = new ReserveStockCommand(order.Id, item);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => Mediator.Send(command));
        
            Assert.Equal("Event publish failed", exception.Message);            
        }
    }
}