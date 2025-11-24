using System;
using Moq;
using OrderManagement.Commands.Payment;
using OrderManagement.Models;
using OrderManagement.Models.Enums;
using OrderManagement.Models.Events.Payments;
using OrderManagement.Repositories;

namespace OrderManagementTests.IntegrationTests
{
    [Trait("Category", "Integration")]
    public class ProcessPaymentHandlerTest : BaseIntegrationTest
    {
        private IRepository<Order> _orderRepository;
        private IRepository<Payment> _paymentRepository;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _orderRepository = GetService<IRepository<Order>>();
            _paymentRepository = GetService<IRepository<Payment>>();
        }

        [Fact]
        public async Task ProcessPaymentOK()
        {
            var user = await GetTestUser();
            var product = await GetTestProduct();

            var order = new Order
            {
                UserId = user.Id,
                Item = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = 2,
                    UnitPrice = product.Price
                },
                OrderStatus = OrderStatus.Placed,
                TotalAmount = product.Price * 2,
                CreatedAt = DateTime.UtcNow
            };

            var createdOrder = await _orderRepository.CreateAsync(order);

            var command = new ProcessPaymentCommand(createdOrder.Id);
            var result = await Mediator.Send(command);
            Assert.True(result);

            var updatedOrder = await _orderRepository.GetByIdAsync(createdOrder.Id);
            Assert.Equal(OrderStatus.Paid, updatedOrder.OrderStatus);

            var payment = await _paymentRepository.FindOneAsync(p => p.OrderId == createdOrder.Id);
            Assert.NotNull(payment);
            Assert.Equal(createdOrder.UserId, payment.UserId);
            Assert.Equal(createdOrder.TotalAmount, payment.Amount);
            Assert.Equal(PaymentStatus.Processed, payment.PaymentStatus);

            MockEventProducer.Verify(x => x.ProduceAsync(
                "payment-processed",
                It.Is<PaymentProcessed>(e =>
                e.PaymentId == payment.Id &&
                e.OrderId == payment.OrderId &&
                e.UserId == payment.UserId &&
                e.Amount == payment.Amount),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessPaymentNokOK_StatusIsCancelled()
        {
            var user = await GetTestUser();
            var product = await GetTestProduct();

            var order = new Order
            {
                UserId = user.Id,
                Item = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = 2,
                    UnitPrice = product.Price
                },
                OrderStatus = OrderStatus.Cancelled,
                TotalAmount = product.Price * 2,
                CreatedAt = DateTime.UtcNow
            };

            var createdOrder = await _orderRepository.CreateAsync(order);
            
            var command = new ProcessPaymentCommand(createdOrder.Id);
            var result = await Mediator.Send(command);
            Assert.False(result);

            var payment = await _paymentRepository.FindOneAsync(p => p.OrderId == createdOrder.Id);
            Assert.Null(payment);

            MockEventProducer.Verify(x => x.ProduceAsync(
                "payment-processed",
                It.IsAny<PaymentProcessed>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessPaymentNokOK_EventPublishFailed()
        {
            var user = await GetTestUser();
            var product = await GetTestProduct();

            var order = new Order
            {
                UserId = user.Id,
                Item = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = 2,
                    UnitPrice = product.Price
                },
                OrderStatus = OrderStatus.Placed,
                TotalAmount = product.Price * 2,
                CreatedAt = DateTime.UtcNow
            };

            var createdOrder = await _orderRepository.CreateAsync(order);

            MockEventProducer.Setup(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Event publish failed"));
            
            var command = new ProcessPaymentCommand(createdOrder.Id);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => Mediator.Send(command));
        
            Assert.Equal("Event publish failed", exception.Message);    
        }
    }
}