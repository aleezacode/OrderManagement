using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderManagement.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using OrderManagement.Configuration;
using System.Linq.Expressions;
using OrderManagement.Exceptions;

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
            try
            {
                await _paymentCollection.InsertOneAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                throw new DocumentCreationFailedException("Payment", ex);
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _paymentCollection.DeleteOneAsync(p => p.Id == id)
                .ContinueWith(task => task.Result.DeletedCount > 0);
        }

        public async Task<Payment?> FindOneAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _paymentCollection.Find(predicate).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _paymentCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Payment> GetByIdAsync(string id)
        {
            var payment = await _paymentCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (payment == null)
            {
                throw new DocumentNotFoundException("Payment", id);
            }

            return payment;
        }

        public async Task UpdateAsync(string id, Payment entity)
        {
            entity.Id = id;

            var result = await _paymentCollection.ReplaceOneAsync(p => p.Id == id, entity);

            if (result.ModifiedCount == 0)
            {
                throw new DocumentUpdatedFailedException("Payment", id);
            }
        }
    }
}