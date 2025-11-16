using System;
using MongoDB.Bson;
using Moq;
using OrderManagement.Commands.Order.Cancellation;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Models.Events;
using OrderManagement.Repositories;

namespace OrderManagementTests.IntegrationTests
{
    [Trait("Category", "Integration")]
    public class CancelOrderByUserHandlerTest : BaseIntegrationTest
    {
        private IRepository<Inventory> _inventoryRepository;
        private IRepository<Order> _orderRepository;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _inventoryRepository = GetService<IRepository<Inventory>>();
            _orderRepository = GetService<IRepository<Order>>();
        }

        [Fact]
        public async Task HandleCancelOrderOK()
        {
            var order = await CreateTestOrder();
            var inventory = await _inventoryRepository.FindOneAsync(x => x.ProductId == order.Item.ProductId);
            var expectedQuantity = inventory.Quantity + order.Item.Quantity;

            var command = new CancelOrderByUserCommand(order.Id, "Cancel order");
            var result = await Mediator.Send(command);
            Assert.True(result);

            var updatedOrder = await _orderRepository.GetByIdAsync(order.Id);
            var updatedInventory = await _inventoryRepository.FindOneAsync(x => x.ProductId == order.Item.ProductId);
            Assert.Equal(OrderStatus.Cancelled, updatedOrder.OrderStatus);
            Assert.Equal(expectedQuantity, updatedInventory.Quantity);

            MockEventProducer.Verify(x => x.ProduceAsync(
                "order-cancelled",
                It.Is<OrderCancelled>(e => 
                            e.OrderId == order.Id &&
                            e.UserId == order.UserId &&
                            e.Reason == "Cancel order" &&
                            e.CancelledBy == "User"),
    It.IsAny<CancellationToken>()),
            Times.Once);
        }

        [Fact]
        public async Task HandleCancelOrderNotOK_OrderNotFound()
        {
            var nonExistentId = ObjectId.GenerateNewId().ToString();
            var command = new CancelOrderByUserCommand(nonExistentId, "Order not found");

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                () => Mediator.Send(command)
            );

            Assert.Equal("Order", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task HandleCancelOrderNotOK_OrderNotInPlacedStatus()
        {
            var order = await CreateTestOrder();
            
            order.OrderStatus = OrderStatus.Paid;
            await _orderRepository.UpdateAsync(order.Id, order);

            var command = new CancelOrderByUserCommand(order.Id, "User requested");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => Mediator.Send(command));

            Assert.Contains("Order has status", exception.Message);
            Assert.Contains("Completed", exception.Message);
        }

        [Fact]
        public async Task HandleCancelOrderNotOK_EventPublishFailed()
        {
            var order = await CreateTestOrder();

            MockEventProducer.Setup(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Event publish failed"));

            var command = new CancelOrderByUserCommand(order.Id, "User requested");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => Mediator.Send(command));
        
            Assert.Equal("Event publish failed", exception.Message);   
        }
    }
}