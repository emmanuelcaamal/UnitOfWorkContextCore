# Changelog

Todos los cambios notables de este proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

---

## [1.9.0] - 2024-12-14

### ✨ Agregado

#### **Factory Pattern para Múltiples Esquemas**
- Nueva interfaz `IUnitOfWorkFactory` para resolución dinámica de contextos
- Nueva clase `UnitOfWorkFactory` con implementación completa
- Método `AddUnitOfWork<TContext>(string contextKey)` para registrar contextos con clave única
- Método `AddUnitOfWorkFactory()` para registrar la factory en DI
- Método `GetUnitOfWork(string contextKey)` para resolución dinámica por clave
- Método `GetUnitOfWork<TContext>()` para resolución type-safe con genéricos
- Método `HasContext(string contextKey)` para validar existencia de contextos
- Método `GetRegisteredContexts()` para debugging y obtener todos los contextos registrados

#### **Documentación**
- README.md completamente renovado con ejemplos y API reference
- README-MultiSchema.md con guía detallada de múltiples esquemas
- QUICKSTART-MultiSchema.md con ejemplos copy-paste listos para usar
- CHANGELOG.md para historial de versiones
- Documentación XML completa en todos los métodos públicos

### 🔧 Mejorado

- Mensajes de error más descriptivos con contextos disponibles
- Thread-safety con locks en el registro global de contextos
- Validación de claves duplicadas con mensajes claros
- Compatibilidad hacia atrás con `AddUnitOfWork<TContext>()` sin breaking changes

### 📚 Casos de Uso Soportados

- Arquitecturas modulares con esquemas separados (account, catalog, payment)
- Multi-tenant con esquemas por cliente
- Reporting multi-esquema
- Migración de datos entre esquemas
- Servicios que trabajan con múltiples contextos simultáneamente

---

## [1.7.x] - Anteriores

### Agregado
- Método `RemoveRange()` en repositorios para eliminación en lote
- Soporte para operaciones en lote con `InsertRange()` y `UpdateRange()`

### Cambiado
- Migración de .NET Core 3.1 a .NET Standard 2.1
- Actualización de Entity Framework Core a versión 8.0

---

## [1.6.x] - Anteriores

### Agregado
- Sistema de paginación con `IPaginate<T>` y `Paginate<T>`
- Integración con DataTables mediante `ToDataTableResponse()`
- Helper para construcción dinámica de predicados LINQ (`PredicateBuilder`)
- Helper para ordenamiento dinámico (`OrderingHelper`)

### Mejorado
- Optimización del caché de repositorios con diccionario por tipo y nombre
- Soporte para proyecciones LINQ con parámetro `selector`
- Control de tracking de Entity Framework con parámetro `enableTracking`

---

## [1.5.x] - Anteriores

### Agregado
- Soporte para includes anidados con `ThenInclude`
- Parámetros opcionales en `Get()` para mayor flexibilidad
- Interfaz `IReadRepository<T>` separada de `IRepository<T>`

### Mejorado
- Separación de responsabilidades: `ReadRepository<T>` y `Repository<T>`
- Mejor manejo de transacciones con try-catch en `Commit()`
- Rollback automático en `Dispose()` si hay transacción pendiente

---

## [1.0.x] - Versión Inicial

### Agregado
- Implementación inicial del patrón Unit of Work
- Implementación inicial del patrón Repository
- Soporte básico para transacciones con `OpenTransaction()` y `Commit()`
- Operaciones CRUD básicas: `Insert()`, `Update()`, `Remove()`
- Métodos de lectura: `Find()` y `Get()`
- Integración con inyección de dependencias de .NET
- Extensión `AddUnitOfWork<TContext>()` para registro en DI

---

## Tipos de Cambios

- `Agregado` - Para nuevas funcionalidades
- `Cambiado` - Para cambios en funcionalidad existente
- `Obsoleto` - Para características que pronto se eliminarán
- `Eliminado` - Para características eliminadas
- `Corregido` - Para corrección de bugs
- `Seguridad` - Para vulnerabilidades de seguridad

---

## Roadmap (Próximas Versiones)

### [1.9.0] - Planeado

#### En Consideración
- [ ] Soporte para transacciones distribuidas con `TransactionScope`
- [ ] Eventos de dominio con `IDomainEvent`
- [ ] Auditoría automática (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- [ ] Soft delete global con `IsDeleted`
- [ ] Especificaciones reutilizables con patrón Specification
- [ ] Cache de segundo nivel con Redis/Memory
- [ ] Soporte para Command/Query Separation (CQRS)
- [ ] Interceptores personalizables
- [ ] Métricas y telemetría con OpenTelemetry

---

## Versionamiento

Este proyecto sigue [Semantic Versioning](https://semver.org/):

- **MAJOR** (1.x.x) - Cambios incompatibles con versiones anteriores
- **MINOR** (x.8.x) - Nueva funcionalidad compatible con versiones anteriores
- **PATCH** (x.x.13) - Corrección de bugs compatible con versiones anteriores

---

## Compatibilidad

| Versión | .NET Target | EF Core | Breaking Changes |
|---------|-------------|---------|------------------|
| 1.9.x   | .NET 8.0    | 8.0.13  | No               |
| 1.8.x   | .NET 8.0    | 8.0.13  | No               |
| 1.7.x   | .NET Standard 2.1 | 5.0 | No               |
| 1.6.x   | .NET Core 3.1 | 3.1    | No               |
| 1.5.x   | .NET Core 3.1 | 3.1    | Sí (Separación Read/Write) |
| 1.0.x   | .NET Core 3.1 | 3.1    | N/A              |

---

## Migración entre Versiones

### De 1.8.x a 1.9.0

**Sin breaking changes.** Puedes actualizar directamente cambiando la versión en PackageReference.

**Cambios:**
- Versión actualizada de 1.8.0.13 a 1.9.0 (semantic versioning estándar)
- Misma funcionalidad, mejor versionamiento

```bash
# Actualizar paquetes
dotnet add package UnitOfWorkContext.Core --version 1.9.0
dotnet add package UnitOfWorkContext.DependencyInjection --version 1.9.0
```

### De 1.7.x a 1.9.0

**Sin breaking changes.** Puedes actualizar directamente.

**Nuevas características disponibles:**
- Factory Pattern para múltiples esquemas (opcional)
- Método `AddUnitOfWork(string key)` (opcional, complementa el existente)
- Método `AddUnitOfWorkFactory()` para habilitar resolución dinámica
- Método `GetRegisteredContexts()` para debugging

**Código existente:** Continúa funcionando sin cambios.

```csharp
// Código v1.7 - Sigue funcionando
services.AddUnitOfWork<AppDbContext>();

// Nuevo en v1.9 - Opcional
services.AddUnitOfWork<AccountContext>("account");
services.AddUnitOfWork<CatalogContext>("catalog");
services.AddUnitOfWorkFactory();
```

### De 1.6.x a 1.7.x

- Cambio de target framework de .NET Core 3.1 a .NET Standard 2.1
- Requiere actualización de proyecto consumidor a .NET Standard 2.1 o superior
- Método `RemoveRange()` ahora disponible

### De 1.5.x a 1.6.x

- Sin breaking changes
- Nuevas características de paginación y helpers disponibles

---

## Soporte

- **Versión Actual:** 1.8.0 (Soporte completo)
- **Versiones Anteriores:** Soporte limitado a bugs críticos

Para reportar issues o sugerir features, visita:
- [GitHub Issues](https://github.com/tu-usuario/UnitOfWorkContextCore/issues)

---

**Última actualización:** 2024-12-14
