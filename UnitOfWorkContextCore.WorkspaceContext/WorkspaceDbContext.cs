using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace UnitOfWorkContextCore.WorkspaceContext
{
    public abstract class WorkspaceDbContext : DbContext, IWorkspaceDbContext
    {
        private string _connectionString;
        //private string _assemblyName;
        public WorkspaceDbContext(string connectionString)
        {
            _connectionString = connectionString;
            //_assemblyName = assemblyName;
        }

        public WorkspaceDbContext(DbContextOptions<WorkspaceDbContext> options) 
            : base (options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
