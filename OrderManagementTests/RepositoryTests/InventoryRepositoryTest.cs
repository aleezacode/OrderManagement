using System;
using MongoDB.Bson;
using MongoDB.Driver;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagementTests
{
    [Trait("Category", "Repository")]
    public class InventoryRepositoryTest : BaseRepositoryTest
    {
        private InventoryRepository _inventoryRepository;
        public InventoryRepositoryTest() : base("Inventory")
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _inventoryRepository = new InventoryRepository(Database, MongoDbSettings);
        }

        private async Task<Inventory> CreateTestInventory()
        {
            var collection = Database.GetCollection<Inventory>("Inventory");
            
            var inventory = new Inventory
            {
                ProductId = "de3dd781d59dffe74f38fec8",
                Quantity = 10
            };
            
            await collection.InsertOneAsync(inventory);
            return inventory;
        }

        [Fact]
        public async Task GetInventoryByIdOK()
        {
            var testInventory = await CreateTestInventory();

            var fetchedInventory = await _inventoryRepository.GetByIdAsync(testInventory.Id);

            Assert.Equal(testInventory.Id, fetchedInventory.Id);
            Assert.Equal(testInventory.ProductId, fetchedInventory.ProductId);
            Assert.Equal(testInventory.Quantity, fetchedInventory.Quantity);
        }

        [Fact]
        public async Task GetInventoryByIdNotOK_ThrowsException()
        {
            var nonExistentId = ObjectId.GenerateNewId().ToString();

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                () => _inventoryRepository.GetByIdAsync(nonExistentId)
            );

            Assert.Equal("Inventory", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task GetInventoryByIdNotOK_IdIsNull_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                () => _inventoryRepository.GetByIdAsync(null)
            );

            Assert.Equal("Inventory", exception.DocumentType);
            Assert.Null(exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task UpdateInventoryOK()
        {
            var testInventory = await CreateTestInventory();
            testInventory.Quantity -= 1;

            await _inventoryRepository.UpdateAsync(testInventory.Id, testInventory);

            var fetchedInventory = await _inventoryRepository.GetByIdAsync(testInventory.Id);

            Assert.NotNull(fetchedInventory);
            Assert.Equal(testInventory.Quantity, fetchedInventory.Quantity);
        }

        [Fact]
        public async Task UpdateInventoryNotOK_ThrowsException()
        {
            var inventory = new Inventory
            {
                ProductId = "de3dd781d59dffe74f38fec8",
                Quantity = 10
            };

            var nonExistentId = ObjectId.GenerateNewId().ToString();

            var exception = await Assert.ThrowsAsync<DocumentUpdatedFailedException>(
                () => _inventoryRepository.UpdateAsync(nonExistentId, inventory)
            );

            Assert.Equal("Inventory", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("Failed to update", exception.Message);
        }
    }
}