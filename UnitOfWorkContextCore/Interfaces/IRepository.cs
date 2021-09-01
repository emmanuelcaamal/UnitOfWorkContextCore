using System.Collections.Generic;

namespace UnitOfWorkContextCore.Interfaces
{
    public interface IRepository<T> : IReadRepository<T> where T : class
    {
        T Insert(T entity);
        T Update(T entity);
        T Remove(T entity);
        void InsertRange(ICollection<T> entities);
        void UpdateRange(ICollection<T> entities);
        void RemoveRange(ICollection<T> entities);
    }
}
