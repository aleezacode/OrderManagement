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
    public class InventoryRepository : IRepository<Inventory>
    {
        private readonly IMongoCollection<Inventory> _inventoryCollection;
        public InventoryRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _inventoryCollection = mongoDatabase.GetCollection<Inventory>(mongoDBSettings.Value.InventoryCollectionName);
        }

        public async Task<Inventory> CreateAsync(Inventory entity)
        {
            await _inventoryCollection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _inventoryCollection.DeleteOneAsync(i => i.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<Inventory?> FindOneAsync(Expression<Func<Inventory, bool>> predicate)
        {
            return await _inventoryCollection.Find(predicate).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            return await _inventoryCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Inventory?> GetByIdAsync(string id)
        {
            return await _inventoryCollection.Find(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(string id, Inventory entity)
        {
            entity.Id = id;
            return await _inventoryCollection.ReplaceOneAsync(i => i.Id == id, entity)
                .ContinueWith(task => task.Result.ModifiedCount > 0);
        }
    }
}