using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderManagement.Models;
using OrderManagement.Repositories;

namespace OrderManagementTests
{
    public class NotificationRepositoryTest : BaseIntegrationTest
    {
        private readonly NotificationRepository _notificationRepository;
        public NotificationRepositoryTest() : base("Notifications")
        {
            _notificationRepository = new NotificationRepository(_mongoDBSettings);
        }

        [Fact]
        public async Task CreateNotificationOK()
        {
            var notification = new Notification
            {
                UserId = "64a7f0c2e1b8c8f5d6a4e9b3",
                Message = "Your order has been shipped.",
                Type = Type.SMS,
                Status = NotificationStatus.Sent
            };

            var createdNotification = await _notificationRepository.CreateAsync(notification);
            Assert.NotNull(createdNotification);
            Assert.Equal(notification.UserId, createdNotification.UserId);
            Assert.Equal(notification.Message, createdNotification.Message);
            Assert.Equal(notification.Type, createdNotification.Type);
            Assert.Equal(notification.Status, createdNotification.Status);
        }

        [Fact]
        public async Task GetNotificationByIdOK()
        {
            var notification = new Notification
            {
                UserId = "64a7f0c2e1b8c8f5d6a4e9b4",
                Message = "Your payment was successful.",
                Type = Type.Email,
                Status = NotificationStatus.Pending
            };

            var createdNotification = await _notificationRepository.CreateAsync(notification);
            var fetchedNotification = await _notificationRepository.GetByIdAsync(createdNotification.Id!);

            Assert.NotNull(fetchedNotification);
            Assert.Equal(createdNotification.Id, fetchedNotification.Id);
            Assert.Equal(createdNotification.UserId, fetchedNotification.UserId);
            Assert.Equal(createdNotification.Message, fetchedNotification.Message);
            Assert.Equal(createdNotification.Type, fetchedNotification.Type);
            Assert.Equal(createdNotification.Status, fetchedNotification.Status);
        }

        [Fact]
        public async Task UpdateNotificationStatusOK()
        {
            var notification = new Notification
            {
                UserId = "64a7f0c2e1b8c8f5d6a4e9b5",
                Message = "Your order is out for delivery.",
                Type = Type.SMS,
                Status = NotificationStatus.Pending
            };

            var createdNotification = await _notificationRepository.CreateAsync(notification);
            createdNotification.Status = NotificationStatus.Sent;

            var updated = await _notificationRepository.UpdateAsync(createdNotification.Id, createdNotification);
            Assert.True(updated);

            var updatedNotification = await _notificationRepository.GetByIdAsync(createdNotification.Id!);
            Assert.NotNull(updatedNotification);
            Assert.Equal(NotificationStatus.Sent, updatedNotification.Status);
        }

        [Fact]
        public async Task DeleteNotificationOK()
        {
            var notification = new Notification
            {
                UserId = "64a7f0c2e1b8c8f5d6a4e9b6",
                Message = "Your subscription has been renewed.",
                Type = Type.Email,
                Status = NotificationStatus.Sent
            };

            var createdNotification = await _notificationRepository.CreateAsync(notification);
            await _notificationRepository.DeleteAsync(createdNotification.Id!);

            var deletedNotification = await _notificationRepository.GetByIdAsync(createdNotification.Id!);
            Assert.Null(deletedNotification);
        }
    }
}