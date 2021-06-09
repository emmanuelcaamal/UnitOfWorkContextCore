using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Linq;
using System.Linq.Expressions;
using UnitOfWorkContextCore.Interfaces;
using UnitOfWorkContextCore.Interfaces.Paging;

namespace UnitOfWorkContextCore
{
    public abstract class ReadRepository<T> : IReadRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public ReadRepository(DbContext context)
        {
            _context = context ?? throw new ArgumentException(nameof(context)); ;
            _dbSet = _context.Set<T>();
        }

        public T Find(Expression<Func<T, bool>> predicate, 
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool enableTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            return query.FirstOrDefault();
        }

        public T Find(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, bool enableTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            if (orderBy != null)
                return orderBy(query).FirstOrDefault();
            return query.FirstOrDefault();
        }

        public IPaginate<T> Get(Expression<Func<T, bool>> predicate, int index = 0, int size = 20, bool enableTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            if (predicate != null) query = query.Where(predicate);

            return query.ToPaginate(index, size, _dbSet.Count());
        }

        public IPaginate<T> Get(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, int index = 0, int size = 20, bool enableTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            if (predicate != null) query = query.Where(predicate);

            return orderBy != null ? orderBy(query).ToPaginate(index, size, _dbSet.Count()) : query.ToPaginate(index, size, _dbSet.Count());
        }

        public IPaginate<T> Get(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Func<IQueryable<T>, IIncludableQueryable<T, object>> include, int index = 0, int size = 20, bool enableTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            return orderBy != null ? orderBy(query).ToPaginate(index, size, _dbSet.Count()) : query.ToPaginate(index, size, _dbSet.Count());
        }

        public IPaginate<T> Get(int index = 0, int size = 20, bool enableTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            return query.ToPaginate(index, size, _dbSet.Count());
        }

        public IPaginate<TResult> Get<TResult>(Expression<Func<T, TResult>> selector, Expression<Func<T, bool>> predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, int index = 0, int size = 20, bool enableTracking = true) where TResult : class
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            return orderBy != null
                ? orderBy(query).Select(selector).ToPaginate(index, size, _dbSet.Count())
                : query.Select(selector).ToPaginate(index, size, _dbSet.Count());
        }

        public IPaginate<T> Get(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>> include, int index = 0, int size = 20, bool enableTracking = true)
        {
            IQueryable<T> query = _dbSet;
            if (!enableTracking) query = query.AsNoTracking();

            if (include != null) query = include(query);

            if (predicate != null) query = query.Where(predicate);

            return query.ToPaginate(index, size, _dbSet.Count());
        }
    }
}
