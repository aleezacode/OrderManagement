using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderManagement.Models;
using OrderManagement.Configuration;
using System.Linq.Expressions;
using OrderManagement.Exceptions;

namespace OrderManagement.Repositories
{
    public class ProductRepository : IRepository<Product>
    {
        private readonly IMongoCollection<Product> _productCollection;

        public ProductRepository(IMongoDatabase database, IOptions<MongoDBSettings> mongoDBSettings)
        {
            _productCollection = database.GetCollection<Product>(mongoDBSettings.Value.ProductsCollectionName);
        }
        //Will not be used
        public Task<Product> CreateAsync(Product entity)
        {
            throw new NotImplementedException();
        }

        //Will not be used
        public Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<Product?> FindOneAsync(Expression<Func<Product, bool>> predicate)
        {
            return await _productCollection.Find(predicate).FirstOrDefaultAsync();
        }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Product> GetByIdAsync(string id)
        {
            var product = await _productCollection.Find(p => p.Id == id).FirstOrDefaultAsync();

            if (product == null)
            {
                throw new DocumentNotFoundException("Product", id);
            }

            return product;
        }

        //Will not be used
        public Task UpdateAsync(string id, Product entity)
        {
            throw new NotImplementedException();
        }
    }
}