using System;
using MongoDB.Bson;
using OrderManagement.Commands.Inventory;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagementTests.IntegrationTests
{
    [Trait("Category", "Integration")]
    public class ReleaseStockHandlerTest : BaseIntegrationTest
    {
        private IRepository<Inventory> _inventoryRepository;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _inventoryRepository = GetService<IRepository<Inventory>>();
        }

        [Fact]
        public async Task HandleReleaseStockOK()
        {
            var product = await GetTestProduct();
            var inventory = await _inventoryRepository.FindOneAsync(x => x.ProductId == product.Id);
            var previousQuantity = inventory.Quantity;

            var command = new ReleaseStockCommand(product.Id, 2);

            var result = await Mediator.Send(command);

            Assert.True(result);

            var updatedInventory = await _inventoryRepository.FindOneAsync(x => x.ProductId == product.Id);
            Assert.True(updatedInventory.Quantity - previousQuantity == 2);
        }

        [Fact]
        public async Task HandleReleaseStockNotOK_InventoryNotFound()
        {
            var invalidId = ObjectId.GenerateNewId().ToString();

            var command = new ReleaseStockCommand(invalidId, 2);

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                () => Mediator.Send(command)
            );

            Assert.Equal("Inventory", exception.DocumentType);
            Assert.Contains("was not found", exception.Message);
        }
    }
}