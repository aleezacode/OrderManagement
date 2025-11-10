using System;
using MongoDB.Bson;
using OrderManagement.Exceptions;
using OrderManagement.Models;
using OrderManagement.Models.Events;
using OrderManagement.Repositories;

namespace OrderManagementTests
{
    public class EventPublishlogRepositoryTest : BaseRepositoryTest
    {
        private readonly EventPublishlogRepository _eventPublishlogRepository;
        public EventPublishlogRepositoryTest() : base("EventPublishlog")
        {
            _eventPublishlogRepository = new EventPublishlogRepository(_mongoDBSettings);
        }

        private async Task<OrderCancelled> CreateEvent()
        {
            var ev = new OrderCancelled
            {
                OrderId = "64a7f0c2e1b8c8f5d6a4e9b3",
                UserId = "64a7f0c2e1b8c8f5d6a4e9c4",
                Reason = "Test",
                CancelledBy = "User"
            };

            return ev;
        }

        [Fact]
        public async Task CreatePublishlogOK()
        {
            var ev = await CreateEvent();

            var publishlog = new EventPublishlog
            {
                OrderId = ev.OrderId,
                EventType = ev.GetType().Name,
                EventMessage = ev.ToString(),
                PublishedAt = DateTime.UtcNow
            };

            var createdPublishlog = await _eventPublishlogRepository.CreateAsync(publishlog);

            Assert.Equal(publishlog.Id, createdPublishlog.Id);
            Assert.Equal(publishlog.OrderId, createdPublishlog.OrderId);
            Assert.Equal(publishlog.EventType, createdPublishlog.EventType);
            Assert.Equal(publishlog.EventMessage, createdPublishlog.EventMessage);
            Assert.Equal(publishlog.PublishedAt, createdPublishlog.PublishedAt);
        }

        [Fact]
        public async Task CreatePublishlogNotOK_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<DocumentCreationFailedException>(
                () => _eventPublishlogRepository.CreateAsync(null)
            );

            Assert.Equal("EventPublishLog", exception.DocumentType);
            Assert.Contains("Failed to create", exception.Message);
        }

        [Fact]
        public async Task GetPublishlogByIdOK()
        {
            var ev = await CreateEvent();

            var publishlog = new EventPublishlog
            {
                OrderId = ev.OrderId,
                EventType = ev.GetType().Name,
                EventMessage = ev.ToString(),
                PublishedAt = DateTime.UtcNow
            };

            var createdPublishlog = await _eventPublishlogRepository.CreateAsync(publishlog);
            var fetchedPublishlog = await _eventPublishlogRepository.GetByIdAsync(createdPublishlog.Id);

            Assert.Equal(createdPublishlog.Id, fetchedPublishlog.Id);
            Assert.Equal(createdPublishlog.OrderId, fetchedPublishlog.OrderId);
            Assert.Equal(createdPublishlog.EventType, fetchedPublishlog.EventType);
            Assert.Equal(createdPublishlog.EventMessage, fetchedPublishlog.EventMessage);
            Assert.True(Math.Abs((createdPublishlog.PublishedAt - fetchedPublishlog.PublishedAt).TotalMilliseconds) < 5);
        }

        [Fact]
        public async Task GetPublishlogByIdNotOK_ThrowsException()
        {
            var nonExistentId = ObjectId.GenerateNewId().ToString();

            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                () => _eventPublishlogRepository.GetByIdAsync(nonExistentId)
            );

            Assert.Equal("EventPublishLog", exception.DocumentType);
            Assert.Equal(nonExistentId, exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }

        [Fact]
        public async Task GetPublishlogByIdNotOK_IdIsNull_ThrowsException()
        {
            var exception = await Assert.ThrowsAsync<DocumentNotFoundException>(
                () => _eventPublishlogRepository.GetByIdAsync(null)
            );

            Assert.Equal("EventPublishLog", exception.DocumentType);
            Assert.Null(exception.DocumentId);
            Assert.Contains("was not found", exception.Message);
        }
    }
}