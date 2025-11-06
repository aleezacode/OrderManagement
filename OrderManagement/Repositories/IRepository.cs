using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using MongoDB.Bson;

namespace OrderManagement.Repositories
{
    public interface IRepository<T>
    {
        Task<T> CreateAsync(T entity);
        Task<T> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
        Task UpdateAsync(string id, T entity);
        Task<bool> DeleteAsync(string id); 

        //Generic methods
        Task<T?> FindOneAsync(Expression<Func<T, bool>> predicate);
    }
}