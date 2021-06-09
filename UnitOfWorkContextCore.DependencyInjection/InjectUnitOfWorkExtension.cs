using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnitOfWorkContextCore.Interfaces;

namespace UnitOfWorkContextCore.DependencyInjection
{
    public static class InjectUnitOfWorkExtension
    {
        public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
            services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();

            return services;
        }

        public static IServiceCollection AddWorkspaceUnitOfWork(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWorkspace, UnitOfWorkspace>();

            return services;
        }
    }
}
