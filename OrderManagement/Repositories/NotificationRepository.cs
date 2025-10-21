using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderManagement.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using OrderManagement.Configuration;

namespace OrderManagement.Repositories
{
    public class NotificationRepository : IRepository<Notification>
    {
        private readonly IMongoCollection<Notification> _notificationCollection;    
        public NotificationRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _notificationCollection = mongoDatabase.GetCollection<Notification>(mongoDBSettings.Value.NotificationsCollectionName);
        }

        public async Task<Notification> CreateAsync(Notification entity)
        {
            await _notificationCollection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _notificationCollection.DeleteOneAsync(n => n.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await _notificationCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(string id)
        {
            return await _notificationCollection.Find(n => n.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(string id, Notification entity)
        {
            entity.Id = id;
            return await _notificationCollection.ReplaceOneAsync(n => n.Id == id, entity)
                .ContinueWith(task => task.Result.ModifiedCount > 0);
        }
    }
}