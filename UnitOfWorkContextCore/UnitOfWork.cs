using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using UnitOfWorkContextCore.Interfaces;

namespace UnitOfWorkContextCore
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext, IDisposable
    {
        private Dictionary<(Type type, string name), object> _repositories;

        private IDbContextTransaction _transaction;

        public UnitOfWork(TContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public TContext Context { get; set; }

        public void Commit()
        {
            try
            {
                Context.SaveChanges();
                if(_transaction != null)
                {
                    _transaction.Commit();
                    Dispose();
                }
            }
            catch (Exception ex)
            { 
                if(_transaction != null)
                {
                    _transaction.Rollback();
                    Dispose();
                }

                throw ex;
            }
        }

        public void Dispose()
        {
            if(_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            Context?.Dispose();
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            return (IRepository<T>)GetGenericRepository(typeof(T), new Repository<T>(Context));
        }

        public void OpenTransaction()
        {
            if(_transaction == null)
            {
                _transaction = Context.Database.BeginTransaction();
            }
        }

        internal object GetGenericRepository(Type type, object repo)
        {
            _repositories ??= new Dictionary<(Type type, string name), object>();

            if(_repositories.TryGetValue((type, repo.GetType().FullName), out var respository)) 
                return respository;

            _repositories.Add((type, repo.GetType().FullName), repo);
            return repo;
        }
    }
}
