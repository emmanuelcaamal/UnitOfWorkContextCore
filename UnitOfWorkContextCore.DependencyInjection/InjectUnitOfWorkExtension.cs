using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using UnitOfWorkContextCore.Interfaces;

namespace UnitOfWorkContextCore.DependencyInjection
{
    public static class InjectUnitOfWorkExtension
    {
        // Registro global de contextos para la factory
        private static readonly Dictionary<string, Type> _contextRegistry = new();

        /// <summary>
        /// Registra un UnitOfWork para el contexto especificado.
        /// Método tradicional para uso con inyección directa de IUnitOfWork&lt;TContext&gt;.
        /// </summary>
        /// <typeparam name="TContext">Tipo del DbContext a registrar</typeparam>
        /// <param name="services">Colección de servicios</param>
        /// <returns>Colección de servicios para encadenamiento</returns>
        public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();
            return services;
        }

        /// <summary>
        /// Registra un UnitOfWork para el contexto especificado con una clave única.
        /// Permite resolución dinámica del contexto usando IUnitOfWorkFactory.
        /// </summary>
        /// <typeparam name="TContext">Tipo del DbContext a registrar</typeparam>
        /// <param name="services">Colección de servicios</param>
        /// <param name="contextKey">Clave única para identificar el contexto (ej: "account", "catalog", "payment")</param>
        /// <returns>Colección de servicios para encadenamiento</returns>
        /// <exception cref="ArgumentNullException">Si contextKey es null o vacío</exception>
        /// <exception cref="InvalidOperationException">Si ya existe un contexto registrado con la misma clave</exception>
        /// <example>
        /// services.AddUnitOfWork&lt;AccountContext&gt;("account");
        /// services.AddUnitOfWork&lt;CatalogContext&gt;("catalog");
        /// services.AddUnitOfWork&lt;PaymentContext&gt;("payment");
        /// </example>
        public static IServiceCollection AddUnitOfWork<TContext>(
            this IServiceCollection services,
            string contextKey)
            where TContext : DbContext
        {
            if (string.IsNullOrWhiteSpace(contextKey))
                throw new ArgumentNullException(nameof(contextKey), "La clave del contexto no puede ser null o vacía");

            // Registrar el UnitOfWork normalmente
            services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();

            // Agregar al registro global de contextos
            lock (_contextRegistry)
            {
                if (_contextRegistry.TryGetValue(contextKey, out var existingType))
                {
                    throw new InvalidOperationException(
                        $"Ya existe un contexto registrado con la clave '{contextKey}'. " +
                        $"Las claves deben ser únicas. Contexto existente: {existingType.Name}");
                }

                _contextRegistry[contextKey] = typeof(TContext);
            }

            return services;
        }

        /// <summary>
        /// Registra la factory de UnitOfWork para resolución dinámica de contextos.
        /// Debe llamarse DESPUÉS de registrar todos los contextos con AddUnitOfWork.
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <returns>Colección de servicios para encadenamiento</returns>
        /// <example>
        /// // Primero registrar contextos
        /// services.AddUnitOfWork&lt;AccountContext&gt;("account");
        /// services.AddUnitOfWork&lt;CatalogContext&gt;("catalog");
        ///
        /// // Luego registrar la factory
        /// services.AddUnitOfWorkFactory();
        ///
        /// // Uso en servicios:
        /// public MyService(IUnitOfWorkFactory factory)
        /// {
        ///     var accountUoW = factory.GetUnitOfWork("account");
        ///     var catalogUoW = factory.GetUnitOfWork&lt;CatalogContext&gt;();
        /// }
        /// </example>
        public static IServiceCollection AddUnitOfWorkFactory(this IServiceCollection services)
        {
            // Registrar el diccionario de contextos como singleton
            services.AddSingleton(_contextRegistry);

            // Registrar la factory como scoped (una instancia por request)
            services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();

            return services;
        }

        /// <summary>
        /// Obtiene la lista de contextos registrados (útil para debugging)
        /// </summary>
        /// <returns>Diccionario de contextos registrados (clave → tipo)</returns>
        public static IReadOnlyDictionary<string, Type> GetRegisteredContexts()
        {
            lock (_contextRegistry)
            {
                return new Dictionary<string, Type>(_contextRegistry);
            }
        }
    }
}
