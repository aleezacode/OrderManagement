using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderManagement.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using OrderManagement.Configuration;
using System.Linq.Expressions;
using MongoDB.Bson;
using OrderManagement.Exceptions;
using Confluent.Kafka;

namespace OrderManagement.Repositories
{
    public class NotificationRepository : IRepository<Notification>
    {
        private readonly IMongoCollection<Notification> _notificationCollection;    
        public NotificationRepository(IMongoDatabase database, IOptions<MongoDBSettings> mongoDBSettings)
        {
            _notificationCollection = database.GetCollection<Notification>(mongoDBSettings.Value.NotificationsCollectionName);
        }

        public async Task<Notification> CreateAsync(Notification entity)
        {
            try
            {
                await _notificationCollection.InsertOneAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                throw new DocumentCreationFailedException("Notification", ex);
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _notificationCollection.DeleteOneAsync(n => n.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<Notification?> FindOneAsync(Expression<Func<Notification, bool>> predicate)
        {
            return await _notificationCollection.Find(predicate).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _notificationCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Notification> GetByIdAsync(string id)
        {
            var notification = await _notificationCollection.Find(n => n.Id == id).FirstOrDefaultAsync();

            if (notification == null)
            {
                throw new DocumentNotFoundException("Notification", id);
            }

            return notification;
        }

        public async Task UpdateAsync(string id, Notification entity)
        {
            entity.Id = id;

            var result = await _notificationCollection.ReplaceOneAsync(n => n.Id == id, entity);

            if (result.ModifiedCount == 0)
            {
                throw new DocumentUpdatedFailedException("Notification", id);
            }
        }
    }
}