using System;
using MongoDB.Bson;
using Moq;
using OrderManagement.Commands.Order;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Models.Events.Orders;
using OrderManagement.Repositories;
using OrderItem = OrderManagement.Commands.Order.OrderItem;

namespace OrderManagementTests.IntegrationTests
{
    [Trait("Category", "Integration")]
    public class PlaceOrderHandlerTest : BaseIntegrationTest
    {
        private IRepository<Order> _orderRepository;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _orderRepository = GetService<IRepository<Order>>();
        }

        [Fact]
        public async Task PlaceOrderOK()
        {
            var testUser = await GetTestUser();
            var testProduct = await GetTestProduct();

            var orderItem = new OrderItem
            {
                ProductId = testProduct.Id,
                UnitPrice = testProduct.Price,
                Quantity = 2
            };
            var command = new PlaceOrderCommand(testUser.Id, orderItem);

            var orderId = await Mediator.Send(command);

            Assert.NotNull(orderId);

            MockEventProducer.Verify(x => x.ProduceAsync(
                "order-placed",
                It.Is<OrderPlaced>(e =>
                            e.OrderId == orderId &&
                            e.UserId == testUser.Id &&
                            e.Item.ProductId == testProduct.Id &&
                            e.Item.Quantity == orderItem.Quantity),
    It.IsAny<CancellationToken>()),
            Times.Once);

            var fetchedOrder = await _orderRepository.GetByIdAsync(orderId);

            Assert.Equal(orderId, fetchedOrder.Id);
            Assert.Equal(command.UserId, fetchedOrder.UserId);
            Assert.Equal(command.Item.ProductId, fetchedOrder.Item.ProductId);
            Assert.Equal(command.Item.UnitPrice, fetchedOrder.Item.UnitPrice);
            Assert.Equal(command.Item.Quantity, fetchedOrder.Item.Quantity);
            Assert.Equal(OrderStatus.Placed, fetchedOrder.OrderStatus);
            
            var expectedTotal = orderItem.UnitPrice * orderItem.Quantity;
            Assert.Equal(expectedTotal, fetchedOrder.TotalAmount);
        }

        [Fact]
        public async Task PlaceOrderNotOK_UserNotFound()
        {
            var nonExistentId = ObjectId.GenerateNewId().ToString();
            var testProduct = await GetTestProduct();

            var orderItem = new OrderItem
            {
                ProductId = testProduct.Id,
                UnitPrice = testProduct.Price,
                Quantity = 2
            };
            var command = new PlaceOrderCommand(nonExistentId, orderItem);

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                () => Mediator.Send(command)
            );

            Assert.Equal("User", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("User with ID " + nonExistentId + " was not found", exception.Message);
        }

        [Fact]
        public async Task PlaceOrderNotOK_ProductNotFound()
        {
            var testUser = await GetTestUser();
            var nonExistentId = ObjectId.GenerateNewId().ToString();

            var orderItem = new OrderItem
            {
                ProductId = nonExistentId,
                UnitPrice = 29.99M,
                Quantity = 2
            };
            var command = new PlaceOrderCommand(testUser.Id, orderItem);

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                () => Mediator.Send(command)
            );

            Assert.Equal("Product", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("Product with ID " + nonExistentId + " was not found", exception.Message);
        }

        [Fact]
        public async Task PlaceOrderNotOK_EventPublishFailed()
        {
            var testUser = await GetTestUser();
            var testProduct = await GetTestProduct();

            MockEventProducer.Setup(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new InvalidOperationException("Event publish failed"));

            var orderItem = new OrderItem
            {
                ProductId = testProduct.Id,
                UnitPrice = testProduct.Price,
                Quantity = 2
            };

            var command = new PlaceOrderCommand(testUser.Id, orderItem);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => Mediator.Send(command));
                
            Assert.Equal("Event publish failed", exception.Message);
        }

        [Fact]
        public async Task PlaceOrderNotOK_OrderItemIsNull()
        {
            var testUser = await GetTestUser();

            var command = new PlaceOrderCommand(testUser.Id, null);

            await Assert.ThrowsAsync<NullReferenceException>(
                () => Mediator.Send(command)
            );
        }
    }
}