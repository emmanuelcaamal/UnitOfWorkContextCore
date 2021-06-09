using System.Collections.Generic;

namespace UnitOfWorkContextCore.Interfaces
{
    public interface IRepository<T> : IReadRepository<T> where T : class
    {
        T Insert(T entity);

        T Update(T entity);

        void InsertRange(ICollection<T> entities);

        void UpdateRange(ICollection<T> entities);
    }
}
