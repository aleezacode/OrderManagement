using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagementTests
{
    //TODO: Add tests for other CRUD operations as well (Add tests for other repos)
    public class OrderRepositoryTest : BaseIntegrationTest
    {
        private readonly OrderRepository _orderRepository;

        public OrderRepositoryTest() : base("Orders")
        {
            _orderRepository = new OrderRepository(_mongoDBSettings);
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnCreatedOrder()
        {
            var order = new Order
            {
                OrderNumber = Guid.NewGuid(),
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
        public async Task GetOrderById_ShouldReturnOrder()
        {
            var order = new Order
            {
                OrderNumber = Guid.NewGuid(),
                UserId = "68f683373d57a355d5e95ab2",
                TotalAmount = 59.98m,
                OrderStatus = OrderStatus.Placed,
            };
            order.Item =new OrderItem
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
    }
}