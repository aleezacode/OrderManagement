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
using OrderManagement.Exceptions;
using OrderManagement.Models;

namespace OrderManagement.Repositories
{
    public class InventoryRepository : IRepository<Inventory>
    {
        private readonly IMongoCollection<Inventory> _inventoryCollection;
        public InventoryRepository(IMongoDatabase database, IOptions<MongoDBSettings> mongoDBSettings)
        {
            _inventoryCollection = database.GetCollection<Inventory>(mongoDBSettings.Value.InventoryCollectionName);
        }

        //Will not be used
        public async Task<Inventory> CreateAsync(Inventory entity)
        {
            throw new NotImplementedException();
        }

        //Will not be used
        public async Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<Inventory> FindOneAsync(Expression<Func<Inventory, bool>> predicate)
        {
            var inventory = await _inventoryCollection.Find(predicate).FirstOrDefaultAsync();

            if (inventory == null)
            {
                throw new DocumentNotFoundException("Inventory", "N/A");
            }

            return inventory;
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            return await _inventoryCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Inventory> GetByIdAsync(string id)
        {
            var inventory = await _inventoryCollection.Find(i => i.Id == id).FirstOrDefaultAsync();

            if (inventory == null)
            {
                throw new DocumentNotFoundException("Inventory", id);
            }

            return inventory;
        }

        public async Task UpdateAsync(string id, Inventory entity)
        {
            entity.Id = id;
            entity.LastUpdatedAt = DateTime.UtcNow;

            var result = await _inventoryCollection.ReplaceOneAsync(i => i.Id == id, entity);

            if (result.ModifiedCount == 0)
            {
                throw new DocumentUpdatedFailedException("Inventory", id);
            }
        }
    }
}