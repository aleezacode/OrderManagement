using System;
using MongoDB.Bson;
using Moq;
using OrderManagement.Commands.Notification;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Models.Events.Notification;
using OrderManagement.Repositories;

namespace OrderManagementTests.IntegrationTests
{
    public class SendNotificationHandlerTest : BaseIntegrationTest
    {
        private IRepository<Notification> _notificationRepository;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _notificationRepository = GetService<IRepository<Notification>>();
        }

        [Fact]
        public async Task HandleSendNotificationOK()
        {
            var order = await CreateTestOrder();

            var command = new SendNotificationCommand(order.Id, "Test message");

            var result = await Mediator.Send(command);

            Assert.True(result);

            MockEventProducer.Verify(x => x.ProduceAsync(
                "notification-sent",
                It.IsAny<NotificationSent>(),
    It.IsAny<CancellationToken>()),
            Times.Once);

            var fetchedNotification = await _notificationRepository.FindOneAsync(x => x.OrderId == order.Id && x.Message == "Test message");

            Assert.NotNull(fetchedNotification);
        }

        [Fact]
        public async Task HandleSendNotificationNotOK_EventPublishFailed()
        {
            var order = await CreateTestOrder();

            MockEventProducer.Setup(x => x.ProduceAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Event publish failed"));

            var command = new SendNotificationCommand(order.Id, "Test Message");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
        () => Mediator.Send(command));
        
            Assert.Equal("Event publish failed", exception.Message);            
        }

        [Fact]
        public async Task HandleSendNotificationNotOK_OrderNotFound()
        {
            var nonExistentOrderId = ObjectId.GenerateNewId().ToString();
            var command = new SendNotificationCommand(nonExistentOrderId, "Test message");

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                () => Mediator.Send(command));

            Assert.Equal("Order", exception.DocumentType);
            Assert.Equal(nonExistentOrderId, exception.DocumentId);
        }
    }
}