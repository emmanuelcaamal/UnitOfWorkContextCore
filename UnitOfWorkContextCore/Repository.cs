using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using UnitOfWorkContextCore.Interfaces;

namespace UnitOfWorkContextCore
{
    public class Repository<T> : ReadRepository<T>, IRepository<T> where T : class
    {
        public Repository(DbContext context) :
            base(context)
        {
        }

        public T Insert(T entity)
        {
            return _dbSet.Add(entity).Entity;
        }

        public T Update(T entity)
        {
            return _dbSet.Update(entity).Entity;
        }

        public void InsertRange(ICollection<T> entities)
        {
            _dbSet.AddRange(entities);
        }

        public void UpdateRange(ICollection<T> entities)
        {
            _dbSet.UpdateRange(entities);
        }

        public T Remove(T entity)
        {
            return _dbSet.Remove(entity).Entity;
        }

        public void RemoveRange(ICollection<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }
    }
}
