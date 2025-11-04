using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderManagement.Configuration;
using OrderManagement.Models;

namespace OrderManagement.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var mongoClient = new MongoClient(mongoDBSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _userCollection = mongoDatabase.GetCollection<User>(mongoDBSettings.Value.UsersCollectionName);
        }
        //Will not be used
        public Task<User> CreateAsync(User entity)
        {
            throw new NotImplementedException();
        }

        //Will not be used
        public Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<User?> FindOneAsync(Expression<Func<User, bool>> predicate)
        {
            return await _userCollection.Find(predicate).FirstOrDefaultAsync();
        }

        //Will not be used
        public Task<IEnumerable<User>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            return await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        //Will not be used
        public Task<bool> UpdateAsync(string id, User entity)
        {
            throw new NotImplementedException();
        }
    }
}