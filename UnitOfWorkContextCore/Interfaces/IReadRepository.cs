using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Linq.Expressions;
using UnitOfWorkContextCore.Interfaces.Paging;

namespace UnitOfWorkContextCore.Interfaces
{
    public interface IReadRepository<T> where T : class
    {
        T Find(Expression<Func<T, bool>> predicate,
           Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
           bool enableTracking = true);

        T Find(Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
           Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
           bool enableTracking = true);

        IPaginate<T> Get(Expression<Func<T, bool>> predicate,
            int index = 0, int size = 20, bool enableTracking = true);

        IPaginate<T> Get(Expression<Func<T, bool>> predicate, 
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include, 
            int index = 0, int size = 20, bool enableTracking = true);

        IPaginate<T> Get(Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            int index = 0, int size = 20, bool enableTracking = true);

        IPaginate<T> Get(Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include,
            int index = 0, int size = 20, bool enableTracking = true);

        IPaginate<T> Get(int index = 0, int size = 20, bool enableTracking = true);

        IPaginate<TResult> Get<TResult>(Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            int index = 0, int size = 20, bool enableTracking = true) where TResult : class;
    }
}
