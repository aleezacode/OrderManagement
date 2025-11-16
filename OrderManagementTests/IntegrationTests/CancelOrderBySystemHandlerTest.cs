using System;
using Moq;
using OrderManagement.Commands.Order.Cancellation;
using OrderManagement.Models;
using OrderManagement.Models.Events;
using OrderManagement.Repositories;

namespace OrderManagementTests.IntegrationTests
{
    [Trait("Category", "Integration")]
    public class CancelOrderBySystemHandlerTest : BaseIntegrationTest
    {
        private IRepository<Order> _orderRepository;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _orderRepository = GetService<IRepository<Order>>();
        }

        [Fact]
        public async Task HandleCancelOrderOK()
        {
            var order = await CreateTestOrder();

            var command = new CancelOrderBySystemCommand(order.Id, "Cancel order");
            var result = await Mediator.Send(command);
            Assert.True(result);

            var updatedOrder = await _orderRepository.GetByIdAsync(order.Id);
            Assert.Equal(OrderStatus.Cancelled, updatedOrder.OrderStatus);

            MockEventProducer.Verify(x => x.ProduceAsync(
                "order-cancelled",
                It.Is<OrderCancelled>(e => 
                            e.OrderId == order.Id &&
                            e.UserId == order.UserId &&
                            e.Reason == "Cancel order" &&
                            e.CancelledBy == "System"),
    It.IsAny<CancellationToken>()),
            Times.Once);
        }

        [Fact]
        public async Task HandleCancelOrderNotOK_EventPublishFailed()
        {
            var order = await CreateTestOrder();

            MockEventProducer.Setup(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Event publish failed"));

            var command = new CancelOrderBySystemCommand(order.Id, "System error");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => Mediator.Send(command));
        
            Assert.Equal("Event publish failed", exception.Message);   
        }
    }
}