using Microsoft.EntityFrameworkCore;
using System;

namespace UnitOfWorkContextCore.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> GetRepository<T>() where T : class;

        void OpenTransaction();

        void Commit();
    }

    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        TContext Context { get; set; }
    }
}
