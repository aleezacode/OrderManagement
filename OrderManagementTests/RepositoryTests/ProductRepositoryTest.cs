using System;
using MongoDB.Bson;
using MongoDB.Driver;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagementTests
{
    public class ProductRepositoryTest : BaseRepositoryTest
    {
        private readonly ProductRepository _productRepository;

        public ProductRepositoryTest() : base("Products")
        {
            _productRepository = new ProductRepository(_mongoDBSettings);
        }

        private async Task<Product> CreateTestProduct()
        {
            var mongoClient = new MongoClient(_mongoDBSettings.Value.ConnectionString);
            var database = mongoClient.GetDatabase(_mongoDBSettings.Value.DatabaseName);
            var collection = database.GetCollection<Product>("Products");
            
            var product = new Product
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = "TestProduct",
                Price = 29.99M
            };
            
            await collection.InsertOneAsync(product);
            return product;
        }

        [Fact]
        public async Task GetProductByIdOK()
        {
            //Hacky test since the repo doesn't use the createAsync method
            var testProduct = await CreateTestProduct();

            var fetchedProduct = await _productRepository.GetByIdAsync(testProduct.Id);

            Assert.NotNull(fetchedProduct);
            Assert.Equal(testProduct.Id, fetchedProduct.Id);
            Assert.Equal(testProduct.Name, fetchedProduct.Name);
            Assert.Equal(testProduct.Price, fetchedProduct.Price);
        }

        [Fact]
        public async Task GetProductByIdNotOK_ThrowsException()
        {
            var nonExistentId = ObjectId.GenerateNewId().ToString();

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                async () => await _productRepository.GetByIdAsync(nonExistentId)
            );

            Assert.Equal("Product", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task GetProductByIdNotOK_IdIsNull_ThrowsExceptions()
        {
            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                async () => await _productRepository.GetByIdAsync(null)
            );

            Assert.Equal("Product", exception.DocumentType);
            Assert.Null(exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }
    }
}