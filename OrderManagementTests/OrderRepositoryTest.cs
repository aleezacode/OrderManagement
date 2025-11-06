using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagementTests
{
    public class OrderRepositoryTest : BaseIntegrationTest
    {
        private readonly OrderRepository _orderRepository;

        public OrderRepositoryTest() : base("Orders")
        {
            _orderRepository = new OrderRepository(_mongoDBSettings);
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
    }
}