using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagementTests
{
    [Trait("Category", "Repository")]
    public class OrderRepositoryTest : BaseRepositoryTest
    {
        private readonly OrderRepository _orderRepository;

        public OrderRepositoryTest() : base("Orders")
        {
            _orderRepository = new OrderRepository(MongoDbSettings);
        }

        [Fact]
        public async Task CreateOrderOK()
        {
            var order = new Order
            {
                UserId = "68f683373d57a355d5e95ab2",
                TotalAmount = 59.98m,
                OrderStatus = OrderStatus.Placed,
            };

            order.Item = new OrderItem
            {
                ProductId = "de3dd781d59dffe74f38fec8",
                Quantity = 2,
                UnitPrice = 29.99m
            };

            var createdOrder = await _orderRepository.CreateAsync(order);
            Assert.NotNull(createdOrder);
        }

        [Fact]
        public async Task CreateOrderNotOk_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<DocumentCreationFailedException>(
                async () => await _orderRepository.CreateAsync(null)
            );

            Assert.Equal("Order", exception.DocumentType);
            Assert.Contains("Failed to create", exception.Message);
        }

        [Fact]
        public async Task GetOrderByIdOK()
        {
            var order = new Order
            {
                UserId = "68f683373d57a355d5e95ab2",
                TotalAmount = 59.98m,
                OrderStatus = OrderStatus.Placed,
            };
            order.Item = new OrderItem
            {
                ProductId = "de3dd781d59dffe74f38fec8",
                Quantity = 2,
                UnitPrice = 29.99m
            };

            await _orderRepository.CreateAsync(order);
            var fetchedOrder = await _orderRepository.GetByIdAsync(order.Id);
            Assert.NotNull(fetchedOrder);
            Assert.Equal(order.Id, fetchedOrder.Id);
        }

        [Fact]
        public async Task GetOrderByIdNotOk_ThrowsException()
        {
            var nonExistentId = ObjectId.GenerateNewId().ToString();

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                async () => await _orderRepository.GetByIdAsync(nonExistentId)
            );

            Assert.Equal("Order", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task GetOrderByIdNotOK_IdIsNull_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                async () => await _orderRepository.GetByIdAsync(null)
            );

            Assert.Equal("Order", exception.DocumentType);
            Assert.Equal(null, exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task DeleteOrderOK()
        {
            var order = new Order
            {
                UserId = "68f683373d57a355d5e95ab2",
                TotalAmount = 59.98m,
                OrderStatus = OrderStatus.Placed,
            };
            order.Item = new OrderItem
            {
                ProductId = "de3dd781d59dffe74f38fec8",
                Quantity = 2,
                UnitPrice = 29.99m
            };

            await _orderRepository.CreateAsync(order);
            var deleteResult = await _orderRepository.DeleteAsync(order.Id);
            Assert.True(deleteResult);
        }

        [Fact]
        public async Task UpdateOrderOK()
        {
            var order = new Order
            {
                UserId = "68f683373d57a355d5e95ab2",
                TotalAmount = 59.98m,
                OrderStatus = OrderStatus.Placed,
            };
            order.Item = new OrderItem
            {
                ProductId = "de3dd781d59dffe74f38fec8",
                Quantity = 2,
                UnitPrice = 29.99m
            };
            await _orderRepository.CreateAsync(order);
            order.OrderStatus = OrderStatus.Failed;

            var exception = await Record.ExceptionAsync(async () =>
            {
                await _orderRepository.UpdateAsync(order.Id!, order);
            });
            Assert.Null(exception);
        }

        [Fact]
        public async Task UpdateOrderNotOK_ThrowsException()
        {
            var order = new Order
            {
                UserId = "68f683373d57a355d5e95ab2",
                TotalAmount = 59.98m,
                OrderStatus = OrderStatus.Placed,
            };
            order.Item = new OrderItem
            {
                ProductId = "de3dd781d59dffe74f38fec8",
                Quantity = 2,
                UnitPrice = 29.99m
            };

            var nonExistentId = ObjectId.GenerateNewId().ToString();

            var exception = await Assert.ThrowsAsync<DocumentUpdatedFailedException>(
                async () => await _orderRepository.UpdateAsync(nonExistentId, order)
            );

            Assert.Equal("Order", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("Failed to update", exception.Message);
        }
    }
}