# UnitOfWork Context Core

[![NuGet](https://img.shields.io/nuget/v/UnitOfWorkContext.Core.svg)](https://www.nuget.org/packages/UnitOfWorkContext.Core/)
[![NuGet](https://img.shields.io/nuget/dt/UnitOfWorkContext.Core.svg)](https://www.nuget.org/packages/UnitOfWorkContext.Core/)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)

**Librería .NET para implementar el patrón Unit of Work y Repository con Entity Framework Core.**

Abstrae las transacciones de base de datos y proporciona una interfaz limpia para trabajar con múltiples contextos y esquemas en proyectos modulares.

---

## 📋 Características

✅ **Patrón Unit of Work** - Gestión centralizada de transacciones
✅ **Patrón Repository** - Operaciones CRUD genéricas
✅ **Múltiples Contextos** - Soporte para múltiples DbContext en el mismo proyecto
✅ **Múltiples Esquemas** - Trabaja con diferentes esquemas de base de datos (account, catalog, payment, etc.)
✅ **Factory Pattern** - Resolución dinámica de contextos
✅ **Paginación** - Sistema integrado de paginación con IPaginate<T>
✅ **LINQ Support** - Expresiones lambda para filtros, ordenamiento e includes
✅ **Transacciones** - Manejo automático de commit/rollback
✅ **Inyección de Dependencias** - Integración nativa con DI de .NET
✅ **Type-Safe** - Fuertemente tipado con genéricos

---

## 📦 Instalación

### **NuGet Package Manager**

```bash
Install-Package UnitOfWorkContext.Core
Install-Package UnitOfWorkContext.DependencyInjection
```

### **.NET CLI**

```bash
dotnet add package UnitOfWorkContext.Core
dotnet add package UnitOfWorkContext.DependencyInjection
```

### **PackageReference**

```xml
<PackageReference Include="UnitOfWorkContext.Core" Version="1.9.0" />
<PackageReference Include="UnitOfWorkContext.DependencyInjection" Version="1.9.0" />
```

---

## 🚀 Inicio Rápido

### **1. Configura tu DbContext**

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
}
```

### **2. Registra en Program.cs**

```csharp
using UnitOfWorkContextCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Registrar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Registrar UnitOfWork
builder.Services.AddUnitOfWork<AppDbContext>();

var app = builder.Build();
```

### **3. Usa en tus Servicios**

```csharp
public class ProductService
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;

    public ProductService(IUnitOfWork<AppDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Product CreateProduct(Product product)
    {
        _unitOfWork.OpenTransaction();
        try
        {
            var repo = _unitOfWork.GetRepository<Product>();
            repo.Insert(product);
            _unitOfWork.Commit(); // SaveChanges + Commit
            return product;
        }
        catch
        {
            _unitOfWork.Dispose(); // Rollback automático
            throw;
        }
    }

    public IPaginate<Product> GetProducts(int page = 1)
    {
        var repo = _unitOfWork.GetRepository<Product>();
        return repo.Get(
            predicate: p => p.IsActive,
            orderBy: q => q.OrderBy(p => p.Name),
            index: page - 1,
            size: 20
        );
    }
}
```

---

## 🏭 Múltiples Esquemas (Nuevo en v1.9)

¿Necesitas trabajar con **múltiples esquemas** en la misma base de datos? Usa el **Factory Pattern**.

### **Escenario:**

```
MiBaseDatos
├── [account]  → Usuarios, Roles
├── [catalog]  → Productos, Categorías
└── [payment]  → Facturas, Pagos
```

### **1. Define tus Contextos con Esquemas**

```csharp
public class AccountContext : DbContext
{
    public AccountContext(DbContextOptions<AccountContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("account"); // ← Esquema específico
        base.OnModelCreating(modelBuilder);
    }
}

public class CatalogContext : DbContext
{
    public CatalogContext(DbContextOptions<CatalogContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog"); // ← Esquema específico
        base.OnModelCreating(modelBuilder);
    }
}
```

### **2. Registra con Claves Únicas**

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registrar DbContext
builder.Services.AddDbContext<AccountContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<CatalogContext>(options =>
    options.UseSqlServer(connectionString));

// Registrar UnitOfWork con CLAVES
builder.Services.AddUnitOfWork<AccountContext>("account");
builder.Services.AddUnitOfWork<CatalogContext>("catalog");

// Registrar la Factory
builder.Services.AddUnitOfWorkFactory();
```

### **3. Usa la Factory**

#### **Opción A: Resolución Dinámica (por clave)**

```csharp
public class MultiSchemaService
{
    private readonly IUnitOfWorkFactory _factory;

    public MultiSchemaService(IUnitOfWorkFactory factory)
    {
        _factory = factory;
    }

    public void ProcessBySchema(string schemaName)
    {
        // Resolver dinámicamente: "account", "catalog", etc.
        var uow = _factory.GetUnitOfWork(schemaName);

        uow.OpenTransaction();
        try
        {
            // Trabajar con el esquema...
            uow.Commit();
        }
        catch
        {
            uow.Dispose();
            throw;
        }
    }
}
```

#### **Opción B: Type-Safe (con genéricos)**

```csharp
public class OrderService
{
    private readonly IUnitOfWorkFactory _factory;

    public OrderService(IUnitOfWorkFactory factory)
    {
        _factory = factory;
    }

    public void CreateOrder(int userId, int productId)
    {
        // Obtener UnitOfWork tipados
        var accountUoW = _factory.GetUnitOfWork<AccountContext>();
        var catalogUoW = _factory.GetUnitOfWork<CatalogContext>();

        // Validar usuario (esquema account)
        var userRepo = accountUoW.GetRepository<User>();
        var user = userRepo.Find(u => u.Id == userId);

        // Validar producto (esquema catalog)
        var productRepo = catalogUoW.GetRepository<Product>();
        var product = productRepo.Find(p => p.Id == productId);

        // Procesar orden...
    }
}
```

📚 **[Ver Guía Completa de Múltiples Esquemas →](README-MultiSchema.md)**
🚀 **[Ver Inicio Rápido con Ejemplos →](QUICKSTART-MultiSchema.md)**

---

## 📖 Uso Detallado

### **Operaciones CRUD**

```csharp
var repo = _unitOfWork.GetRepository<Product>();

// CREATE
var product = new Product { Name = "Laptop", Price = 999.99m };
repo.Insert(product);

// READ
var product = repo.Find(p => p.Id == 1);
var products = repo.Get(
    predicate: p => p.Price > 100,
    orderBy: q => q.OrderBy(p => p.Name),
    include: q => q.Include(p => p.Category)
);

// UPDATE
product.Price = 899.99m;
repo.Update(product);

// DELETE
repo.Remove(product);

// Commit cambios
_unitOfWork.Commit();
```

### **Operaciones en Lote**

```csharp
var repo = _unitOfWork.GetRepository<Product>();

// Insertar múltiples
var products = new List<Product>
{
    new Product { Name = "Product 1" },
    new Product { Name = "Product 2" }
};
repo.InsertRange(products);

// Actualizar múltiples
repo.UpdateRange(updatedProducts);

// Eliminar múltiples
repo.RemoveRange(productsToDelete);

_unitOfWork.Commit();
```

### **Paginación**

```csharp
var repo = _unitOfWork.GetRepository<Product>();

var result = repo.Get(
    predicate: p => p.IsActive,
    orderBy: q => q.OrderByDescending(p => p.CreatedAt),
    index: 0,      // Página 0 (primera página)
    size: 20       // 20 items por página
);

Console.WriteLine($"Total: {result.Count}");
Console.WriteLine($"Páginas: {result.Pages}");
Console.WriteLine($"Tiene anterior: {result.HasPrevious}");
Console.WriteLine($"Tiene siguiente: {result.HasNext}");

foreach (var product in result.Items)
{
    Console.WriteLine(product.Name);
}
```

### **Includes y Proyecciones**

```csharp
var repo = _unitOfWork.GetRepository<Product>();

// Eager Loading
var products = repo.Get(
    include: q => q.Include(p => p.Category)
                   .ThenInclude(c => c.ParentCategory)
);

// Proyecciones (Select)
var productDtos = repo.Get(
    selector: p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        CategoryName = p.Category.Name
    }
);
```

### **Tracking Control**

```csharp
var repo = _unitOfWork.GetRepository<Product>();

// Sin tracking (mejor rendimiento para consultas)
var products = repo.Get(enableTracking: false);

// Con tracking (necesario para updates)
var product = repo.Find(p => p.Id == 1, enableTracking: true);
product.Price = 999.99m;
repo.Update(product);
```

### **Transacciones Manuales**

```csharp
_unitOfWork.OpenTransaction();
try
{
    var productRepo = _unitOfWork.GetRepository<Product>();
    var categoryRepo = _unitOfWork.GetRepository<Category>();

    // Múltiples operaciones
    productRepo.Insert(newProduct);
    categoryRepo.Update(category);

    // Commit todo junto
    _unitOfWork.Commit();
}
catch (Exception)
{
    // Rollback automático
    _unitOfWork.Dispose();
    throw;
}
```

---

## 🎨 Integración con DataTables

Soporte nativo para jQuery DataTables:

```csharp
[HttpPost("datatable")]
public IActionResult GetDataTable([FromBody] DataTableRequest request)
{
    var repo = _unitOfWork.GetRepository<Product>();

    var result = repo.Get(
        predicate: p => p.Name.Contains(request.Search),
        orderBy: q => q.OrderBy(p => p.Name),
        index: request.Start / request.Length,
        size: request.Length
    );

    return Ok(result.ToDataTableResponse(request.Draw));
}
```

---

## 📚 API Reference

### **IUnitOfWork&lt;TContext&gt;**

| Método | Descripción |
|--------|-------------|
| `GetRepository<T>()` | Obtiene un repositorio para la entidad T |
| `OpenTransaction()` | Inicia una transacción de base de datos |
| `Commit()` | Guarda cambios y confirma la transacción |
| `Dispose()` | Libera recursos y hace rollback si es necesario |
| `Context` | Acceso al DbContext subyacente |

### **IRepository&lt;T&gt;**

#### **Escritura**

| Método | Descripción |
|--------|-------------|
| `Insert(T entity)` | Inserta una entidad |
| `Update(T entity)` | Actualiza una entidad |
| `Remove(T entity)` | Elimina una entidad |
| `InsertRange(ICollection<T>)` | Inserta múltiples entidades |
| `UpdateRange(ICollection<T>)` | Actualiza múltiples entidades |
| `RemoveRange(ICollection<T>)` | Elimina múltiples entidades |

#### **Lectura**

| Método | Descripción |
|--------|-------------|
| `Find(Expression<Func<T, bool>>)` | Busca una entidad por predicado |
| `Get(...)` | Obtiene colección paginada con filtros |

**Parámetros de Get():**
- `predicate` - Filtro LINQ (Where)
- `orderBy` - Ordenamiento (OrderBy/ThenBy)
- `include` - Eager loading (Include/ThenInclude)
- `selector` - Proyección (Select)
- `index` - Índice de página (0-based)
- `size` - Tamaño de página
- `enableTracking` - Habilitar tracking de EF Core

### **IUnitOfWorkFactory** (Nuevo)

| Método | Descripción |
|--------|-------------|
| `GetUnitOfWork(string key)` | Obtiene UnitOfWork por clave |
| `GetUnitOfWork<TContext>()` | Obtiene UnitOfWork tipado |
| `HasContext(string key)` | Verifica si existe un contexto |

### **Extensiones de DI**

| Método | Descripción |
|--------|-------------|
| `AddUnitOfWork<TContext>()` | Registra UnitOfWork tradicional |
| `AddUnitOfWork<TContext>(string key)` | Registra UnitOfWork con clave |
| `AddUnitOfWorkFactory()` | Registra la factory de contextos |
| `GetRegisteredContexts()` | Obtiene contextos registrados |

---

## 🗂️ Estructura del Proyecto

```
UnitOfWorkContextCore/
├── UnitOfWork.cs                      - Implementación del patrón
├── UnitOfWorkFactory.cs               - Factory para múltiples contextos
├── Repository.cs                      - Repositorio de escritura
├── ReadRepository.cs                  - Repositorio de lectura
├── Interfaces/
│   ├── IUnitOfWork.cs                - Interfaz principal
│   ├── IUnitOfWorkFactory.cs         - Interfaz de factory
│   ├── IRepository.cs                - Interfaz de repositorio
│   └── IReadRepository.cs            - Interfaz de lectura
├── Paging/
│   ├── IPaginate.cs                  - Interfaz de paginación
│   ├── Paginate.cs                   - Implementación de paginación
│   └── PaginateExtensions.cs         - Extensiones de paginación
└── Helpers/
    ├── PredicateBuilder.cs           - Constructor de predicados LINQ
    └── OrderingHelper.cs             - Helper de ordenamiento

UnitOfWorkContextCore.DependencyInjection/
└── InjectUnitOfWorkExtension.cs      - Extensiones para DI
```

---

## 🔧 Requisitos

- **.NET 8.0** o superior
- **Entity Framework Core 8.0** o superior
- **Microsoft.Extensions.DependencyInjection**

---

## 📝 Ejemplos Completos

### **Ejemplo 1: Servicio Simple**

```csharp
public class CategoryService
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;

    public CategoryService(IUnitOfWork<AppDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Category Create(string name)
    {
        _unitOfWork.OpenTransaction();
        try
        {
            var repo = _unitOfWork.GetRepository<Category>();
            var category = new Category { Name = name };
            repo.Insert(category);
            _unitOfWork.Commit();
            return category;
        }
        catch
        {
            _unitOfWork.Dispose();
            throw;
        }
    }

    public List<Category> GetAll()
    {
        var repo = _unitOfWork.GetRepository<Category>();
        return repo.Get(orderBy: q => q.OrderBy(c => c.Name))
                   .Items.ToList();
    }
}
```

### **Ejemplo 2: Servicio con Relaciones**

```csharp
public class ProductService
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;

    public ProductService(IUnitOfWork<AppDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Product CreateWithCategory(string productName, int categoryId)
    {
        _unitOfWork.OpenTransaction();
        try
        {
            // Validar categoría existe
            var categoryRepo = _unitOfWork.GetRepository<Category>();
            var category = categoryRepo.Find(c => c.Id == categoryId);
            if (category == null)
                throw new Exception("Categoría no encontrada");

            // Crear producto
            var productRepo = _unitOfWork.GetRepository<Product>();
            var product = new Product
            {
                Name = productName,
                CategoryId = categoryId
            };
            productRepo.Insert(product);

            _unitOfWork.Commit();
            return product;
        }
        catch
        {
            _unitOfWork.Dispose();
            throw;
        }
    }

    public IPaginate<ProductDto> SearchProducts(string searchTerm, int page)
    {
        var repo = _unitOfWork.GetRepository<Product>();

        return repo.Get(
            selector: p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryName = p.Category.Name
            },
            predicate: p => p.Name.Contains(searchTerm),
            orderBy: q => q.OrderBy(p => p.Name),
            include: q => q.Include(p => p.Category),
            index: page - 1,
            size: 20
        );
    }
}
```

---

## 🆕 Novedades v1.9

### ✨ **Factory Pattern para Múltiples Esquemas**

- Nuevo `IUnitOfWorkFactory` para resolución dinámica de contextos
- Soporte para múltiples DbContext con esquemas diferentes
- Métodos `AddUnitOfWork(string key)` y `AddUnitOfWorkFactory()`
- Validación de contextos con `HasContext()`
- Thread-safe con locks en registro de contextos
- Método `GetRegisteredContexts()` para inspección y debugging

### 🔧 **Mejoras**

- Mensajes de error más descriptivos con contextos disponibles
- Validación de claves duplicadas al registrar contextos
- Documentación completa con 3 guías especializadas
- Backward compatible con versiones anteriores (sin breaking changes)

---

## 📖 Documentación Adicional

- **[Guía de Múltiples Esquemas](README-MultiSchema.md)** - Documentación detallada del Factory Pattern
- **[Inicio Rápido Multi-Esquema](QUICKSTART-MultiSchema.md)** - Ejemplos copy-paste listos para usar
- **[Changelog](CHANGELOG.md)** - Historial de versiones y cambios

---

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Por favor:

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

---

## 📄 Licencia

Este proyecto está bajo licencia MIT. Ver archivo [LICENSE](LICENSE) para más detalles.

---

## 💡 Soporte

Si encuentras algún problema o tienes sugerencias:

- 🐛 [Reportar Bug](https://github.com/tu-usuario/UnitOfWorkContextCore/issues)
- 💬 [Discusiones](https://github.com/tu-usuario/UnitOfWorkContextCore/discussions)
- 📧 Email: tu-email@ejemplo.com

---

## 🙏 Agradecimientos

Basado en el patrón Unit of Work descrito por Martin Fowler y las mejores prácticas de arquitectura de software .NET.

---

**Hecho con ❤️ para la comunidad .NET**
