using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitOfWorkContextCore.Interfaces;

namespace UnitOfWorkContextCore
{
    /// <summary>
    /// Implementación de la Factory para resolver múltiples UnitOfWork.
    /// Permite trabajar con múltiples DbContext (esquemas) de forma dinámica.
    /// </summary>
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _contextRegistry;

        /// <summary>
        /// Constructor de la factory
        /// </summary>
        /// <param name="serviceProvider">Service provider de la aplicación</param>
        /// <param name="contextRegistry">Diccionario con el registro de contextos y sus claves</param>
        public UnitOfWorkFactory(
            IServiceProvider serviceProvider,
            Dictionary<string, Type> contextRegistry)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _contextRegistry = contextRegistry ?? throw new ArgumentNullException(nameof(contextRegistry));
        }

        /// <summary>
        /// Obtiene un UnitOfWork por clave de contexto
        /// </summary>
        public IUnitOfWork GetUnitOfWork(string contextKey)
        {
            if (string.IsNullOrWhiteSpace(contextKey))
                throw new ArgumentNullException(nameof(contextKey), "La clave del contexto no puede ser null o vacía");

            if (!_contextRegistry.TryGetValue(contextKey, out var contextType))
            {
                var availableKeys = _contextRegistry.Keys.Any()
                    ? string.Join(", ", _contextRegistry.Keys.Select(k => $"'{k}'"))
                    : "ninguno";

                throw new InvalidOperationException(
                    $"No se encontró un contexto registrado con la clave '{contextKey}'. " +
                    $"Contextos disponibles: {availableKeys}. " +
                    $"Asegúrate de haber llamado AddUnitOfWork<TContext>(\"{contextKey}\") en la configuración.");
            }

            // Construir el tipo genérico IUnitOfWork<TContext>
            var unitOfWorkType = typeof(IUnitOfWork<>).MakeGenericType(contextType);

            // Resolver del contenedor de DI
            var unitOfWork = _serviceProvider.GetService(unitOfWorkType);

            if (unitOfWork == null)
            {
                throw new InvalidOperationException(
                    $"No se pudo resolver IUnitOfWork<{contextType.Name}> desde el contenedor de DI. " +
                    $"Asegúrate de haber registrado el contexto con AddUnitOfWork<{contextType.Name}>(\"{contextKey}\").");
            }

            return (IUnitOfWork)unitOfWork;
        }

        /// <summary>
        /// Obtiene un UnitOfWork tipado genéricamente
        /// </summary>
        public IUnitOfWork<TContext> GetUnitOfWork<TContext>() where TContext : DbContext
        {
            var unitOfWork = _serviceProvider.GetService<IUnitOfWork<TContext>>();

            if (unitOfWork == null)
            {
                throw new InvalidOperationException(
                    $"No se pudo resolver IUnitOfWork<{typeof(TContext).Name}> desde el contenedor de DI. " +
                    $"Asegúrate de haber registrado el contexto con AddUnitOfWork<{typeof(TContext).Name}>() o " +
                    $"AddUnitOfWork<{typeof(TContext).Name}>(\"clave\") en la configuración de servicios.");
            }

            return unitOfWork;
        }

        /// <summary>
        /// Verifica si existe un contexto registrado
        /// </summary>
        public bool HasContext(string contextKey)
        {
            return !string.IsNullOrWhiteSpace(contextKey) && _contextRegistry.ContainsKey(contextKey);
        }
    }
}
