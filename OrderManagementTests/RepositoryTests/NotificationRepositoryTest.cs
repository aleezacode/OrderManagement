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
    public class NotificationRepositoryTest : BaseRepositoryTest
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
                OrderId = "64a7f0c2e1b8c8f5d6a4e9c4",
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
            Assert.Equal(notification.OrderId, createdNotification.OrderId);
        }

        [Fact]
        public async Task CreateNotificationNotOK_ExceptionThrown()
        {
            var exception = await Assert.ThrowsAsync<DocumentCreationFailedException>(
                () => _notificationRepository.CreateAsync(null)
            );

            Assert.Equal("Notification", exception.DocumentType);
            Assert.Contains("Failed to create", exception.Message);
        }

        [Fact]
        public async Task GetNotificationByIdOK()
        {
            var notification = new Notification
            {
                UserId = "64a7f0c2e1b8c8f5d6a4e9b4",
                OrderId = "64a7f0c2e1b8c8f5d6a4e9c4",
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
            Assert.Equal(createdNotification.OrderId, fetchedNotification.OrderId);
        }

        [Fact]
        public async Task GetNotificationByIdNotOK_ExceptionThrown()
        {
            var nonExistentId = ObjectId.GenerateNewId().ToString();

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                () => _notificationRepository.GetByIdAsync(nonExistentId)
            );

            Assert.Equal("Notification", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task GetNotificationByIdNotOk_IdIsNull_ExceptionThrown()
        {

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                () => _notificationRepository.GetByIdAsync(null)
            );

            Assert.Equal("Notification", exception.DocumentType);
            Assert.Null(exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task UpdateNotificationStatusOK()
        {
            var notification = new Notification
            {
                UserId = "64a7f0c2e1b8c8f5d6a4e9b5",
                OrderId = "64a7f0c2e1b8c8f5d6a4e9c4",
                Message = "Your order is out for delivery.",
                Type = Type.SMS,
                Status = NotificationStatus.Pending
            };

            var createdNotification = await _notificationRepository.CreateAsync(notification);
            createdNotification.Status = NotificationStatus.Sent;

            await _notificationRepository.UpdateAsync(createdNotification.Id!, createdNotification);

            var updatedNotification = await _notificationRepository.GetByIdAsync(createdNotification.Id!);
            Assert.NotNull(updatedNotification);
            Assert.Equal(NotificationStatus.Sent, updatedNotification.Status);
        }

        [Fact]
        public async Task UpdateNotificationNotOk_ExceptionThrown()
        {
            var notification = new Notification
            {
                UserId = "64a7f0c2e1b8c8f5d6a4e9b5",
                OrderId = "64a7f0c2e1b8c8f5d6a4e9c4",
                Message = "Updating notification",
                Type = Type.Email,
                Status = NotificationStatus.Sent
            };

            var nonExistentId = ObjectId.GenerateNewId().ToString();

            var exception = await Assert.ThrowsAsync<DocumentUpdatedFailedException>(
                () => _notificationRepository.UpdateAsync(nonExistentId, notification)
            );

            Assert.Equal("Notification", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("Failed to update", exception.Message);
        }

        [Fact]
        public async Task DeleteNotificationOK()
        {
            var notification = new Notification
            {
                UserId = "64a7f0c2e1b8c8f5d6a4e9b6",
                OrderId = "64a7f0c2e1b8c8f5d6a4e9c4",
                Message = "Your subscription has been renewed.",
                Type = Type.Email,
                Status = NotificationStatus.Sent
            };

            var createdNotification = await _notificationRepository.CreateAsync(notification);
            await _notificationRepository.DeleteAsync(createdNotification.Id!);

            var exception = await Record.ExceptionAsync(async() =>
            {
                await _notificationRepository.GetByIdAsync(createdNotification.Id);
            });
            Assert.NotNull(exception);
            Assert.Equal(typeof(DocumentNotFoundException), exception.GetType());
        }
    }
}