using Microsoft.EntityFrameworkCore;

namespace UnitOfWorkContextCore.Interfaces
{
    /// <summary>
    /// Factory para resolver múltiples UnitOfWork dinámicamente.
    /// Permite trabajar con múltiples DbContext (esquemas) en el mismo proyecto.
    /// </summary>
    public interface IUnitOfWorkFactory
    {
        /// <summary>
        /// Obtiene un UnitOfWork por clave/nombre de contexto (ej: "account", "catalog", "payment").
        /// Útil para resolución dinámica en runtime.
        /// </summary>
        /// <param name="contextKey">Clave única del contexto registrada con AddUnitOfWork</param>
        /// <returns>Instancia de IUnitOfWork para el contexto especificado</returns>
        /// <exception cref="System.ArgumentNullException">Si contextKey es null o vacío</exception>
        /// <exception cref="System.InvalidOperationException">Si no existe un contexto registrado con la clave</exception>
        IUnitOfWork GetUnitOfWork(string contextKey);

        /// <summary>
        /// Obtiene un UnitOfWork tipado por TContext.
        /// Proporciona type-safety en tiempo de compilación.
        /// </summary>
        /// <typeparam name="TContext">Tipo del DbContext</typeparam>
        /// <returns>Instancia tipada de IUnitOfWork&lt;TContext&gt;</returns>
        /// <exception cref="System.InvalidOperationException">Si no se ha registrado el contexto</exception>
        IUnitOfWork<TContext> GetUnitOfWork<TContext>() where TContext : DbContext;

        /// <summary>
        /// Verifica si existe un contexto registrado con la clave especificada.
        /// </summary>
        /// <param name="contextKey">Clave del contexto a verificar</param>
        /// <returns>true si existe el contexto; false en caso contrario</returns>
        bool HasContext(string contextKey);
    }
}
