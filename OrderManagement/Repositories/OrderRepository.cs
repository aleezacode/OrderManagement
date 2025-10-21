using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderManagement.Models;
using OrderManagement.Configuration;

namespace OrderManagement.Repositories
{
    public class OrderRepository : IRepository<Order>
    {
        private readonly IMongoCollection<Order> _orderCollection;
        public OrderRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _orderCollection = mongoDatabase.GetCollection<Order>(mongoDBSettings.Value.OrdersCollectionName);
        }

        public async Task<Order> CreateAsync(Order entity)
        {
            await _orderCollection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _orderCollection.DeleteOneAsync(o => o.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _orderCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(string id)
        {
            return await _orderCollection.Find(o => o.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(string id, Order entity)
        {
            entity.Id = id;
            return await _orderCollection.ReplaceOneAsync(o => o.Id == id, entity)
                .ContinueWith(task => task.Result.ModifiedCount > 0);
        }
        
    }
}