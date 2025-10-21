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
    public class PaymentRepository : IRepository<Payment>
    {
        private readonly IMongoCollection<Payment> _paymentCollection;
        public PaymentRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _paymentCollection = mongoDatabase.GetCollection<Payment>(mongoDBSettings.Value.PaymentsCollectionName);
        }

        public async Task<Payment> CreateAsync(Payment entity)
        {
            await _paymentCollection.InsertOneAsync(entity);
            return entity;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _paymentCollection.DeleteOneAsync(p => p.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _paymentCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(string id)
        {
            return await _paymentCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateAsync(string id, Payment entity)
        {
            entity.Id = id;
            return await _paymentCollection.ReplaceOneAsync(p => p.Id == id, entity)
                .ContinueWith(task => task.Result.ModifiedCount > 0);
        }
    }
}