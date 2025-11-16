using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderManagement.Models;
using OrderManagement.Configuration;
using System.Linq.Expressions;
using MongoDB.Bson;
using OrderManagement.Exceptions;

namespace OrderManagement.Repositories
{
    public class OrderRepository : IRepository<Order>
    {
        private readonly IMongoCollection<Order> _orderCollection;
        public OrderRepository(IMongoDatabase database, IOptions<MongoDBSettings> mongoDBSettings)
        {
            _orderCollection = database.GetCollection<Order>(mongoDBSettings.Value.OrdersCollectionName);
        }

        public async Task<Order> CreateAsync(Order entity)
        {
            try
            {
                await _orderCollection.InsertOneAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                throw new DocumentCreationFailedException("Order", ex);
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _orderCollection.DeleteOneAsync(o => o.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<Order?> FindOneAsync(Expression<Func<Order, bool>> predicate)
        {
            return await _orderCollection.Find(predicate).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _orderCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Order> GetByIdAsync(string id)
        {
            var order = await _orderCollection.Find(o => o.Id == id).FirstOrDefaultAsync();

            if (order == null)
            {
                throw new DocumentNotFoundException("Order", id);
            }

            return order;
        }

        public async Task UpdateAsync(string id, Order entity)
        {
            entity.Id = id;
            entity.UpdatedAt = DateTime.UtcNow;

            var result = await _orderCollection.ReplaceOneAsync(o => o.Id == id, entity);

            if (result.ModifiedCount == 0)
            {
                throw new DocumentUpdatedFailedException("Order", id);
            }
        }
        
    }
}