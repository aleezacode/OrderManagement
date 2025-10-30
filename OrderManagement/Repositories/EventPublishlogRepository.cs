using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using OrderManagement.Configuration;
using OrderManagement.Models;

namespace OrderManagement.Repositories
{
    public class EventPublishlogRepository : IRepository<EventPublishlog>
    {
        private readonly IMongoCollection<EventPublishlog> _eventPublishlogCollection;
        public EventPublishlogRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _eventPublishlogCollection = mongoDatabase.GetCollection<EventPublishlog>(mongoDBSettings.Value.EventPublishLogCollectionName);
        }

        public async Task<EventPublishlog> CreateAsync(EventPublishlog entity)
        {
            await _eventPublishlogCollection.InsertOneAsync(entity);
            return entity;
        }

        //Will not be used
        public async Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<EventPublishlog?> FindOneAsync(Expression<Func<EventPublishlog, bool>> predicate)
        {
            return await _eventPublishlogCollection.Find(predicate).FirstOrDefaultAsync();
        }

        //Will not be used
        public async Task<IEnumerable<EventPublishlog>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<EventPublishlog?> GetByIdAsync(string id)
        {
            return await _eventPublishlogCollection.Find(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(string id, EventPublishlog entity)
        {
            entity.Id = id;
            return await _eventPublishlogCollection.ReplaceOneAsync(i => i.Id == id, entity)
                .ContinueWith(task => task.Result.ModifiedCount > 0);
        }
    }
}