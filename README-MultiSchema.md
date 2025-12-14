# 🏭 Múltiples Esquemas con Factory Pattern

Esta guía muestra cómo implementar **múltiples DbContext con esquemas diferentes** en un mismo proyecto usando **Unit of Work Factory Pattern**.

## 📋 Tabla de Contenido

- [Problema que Resuelve](#problema-que-resuelve)
- [Solución: Factory Pattern](#solución-factory-pattern)
- [Configuración Paso a Paso](#configuración-paso-a-paso)
- [Ejemplos de Uso](#ejemplos-de-uso)
- [Casos de Uso Comunes](#casos-de-uso-comunes)
- [API Reference](#api-reference)

---

## 🎯 Problema que Resuelve

Cuando trabajas con **arquitectura modular** donde diferentes módulos se conectan a la misma base de datos pero con **esquemas diferentes**:

```
MiBaseDatos
├── [account]  → Usuarios, Roles, Permisos
├── [catalog]  → Productos, Categorías
├── [payment]  → Facturas, Transacciones
└── [shipping] → Envíos, Direcciones
```

**Problema tradicional:**

```csharp
// ❌ Esto NO funciona - el último sobrescribe los anteriores
services.AddUnitOfWork<AccountContext>();
services.AddUnitOfWork<CatalogContext>();  // Sobrescribe AccountContext
services.AddUnitOfWork<PaymentContext>();  // Sobrescribe CatalogContext
```

**Solución con Factory:**

```csharp
// ✅ Esto SÍ funciona - cada contexto tiene su propia clave
services.AddUnitOfWork<AccountContext>("account");
services.AddUnitOfWork<CatalogContext>("catalog");
services.AddUnitOfWork<PaymentContext>("payment");
services.AddUnitOfWorkFactory(); // Registrar la factory
```

---

## 🏭 Solución: Factory Pattern

La **Factory** permite:

✅ **Múltiples contextos** en el mismo proyecto
✅ **Resolución dinámica** por clave string
✅ **Type-safe** con genéricos
✅ **Backward compatible** con código existente
✅ **Sin hardcodear contextos** en la librería

---

## 🚀 Configuración Paso a Paso

### **Paso 1: Crear tus DbContext con Esquemas**

En tu proyecto (NO en UnitOfWorkContextCore):

```csharp
// AccountContext.cs
public class AccountContext : DbContext
{
    public AccountContext(DbContextOptions<AccountContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("account"); // ← Esquema
        base.OnModelCreating(modelBuilder);
    }
}

// CatalogContext.cs
public class CatalogContext : DbContext
{
    public CatalogContext(DbContextOptions<CatalogContext> options)
        : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("catalog"); // ← Esquema
        base.OnModelCreating(modelBuilder);
    }
}

// PaymentContext.cs
public class PaymentContext : DbContext
{
    public PaymentContext(DbContextOptions<PaymentContext> options)
        : base(options) { }

    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("payment"); // ← Esquema
        base.OnModelCreating(modelBuilder);
    }
}
```

---

### **Paso 2: Configurar en Program.cs / Startup.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using UnitOfWorkContextCore.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Connection string (misma base de datos para todos los esquemas)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 1️⃣ Registrar los DbContext
builder.Services.AddDbContext<AccountContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<CatalogContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<PaymentContext>(options =>
    options.UseSqlServer(connectionString));

// 2️⃣ Registrar UnitOfWork para CADA contexto CON CLAVE
builder.Services.AddUnitOfWork<AccountContext>("account");
builder.Services.AddUnitOfWork<CatalogContext>("catalog");
builder.Services.AddUnitOfWork<PaymentContext>("payment");

// 3️⃣ Registrar la Factory (UNA VEZ al final)
builder.Services.AddUnitOfWorkFactory();

var app = builder.Build();
app.Run();
```

**appsettings.json:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MiBaseDatos;Trusted_Connection=true;"
  }
}
```

---

## 💡 Ejemplos de Uso

### **Ejemplo 1: Usar Factory con Clave (Dinámico)**

Útil cuando el esquema se determina en runtime:

```csharp
public class DynamicSchemaService
{
    private readonly IUnitOfWorkFactory _factory;

    public DynamicSchemaService(IUnitOfWorkFactory factory)
    {
        _factory = factory;
    }

    public void ProcessBySchema(string schemaName, int entityId)
    {
        // Resolver dinámicamente por clave
        var unitOfWork = _factory.GetUnitOfWork(schemaName);

        unitOfWork.OpenTransaction();
        try
        {
            // Hacer operaciones...
            unitOfWork.Commit();
        }
        catch
        {
            unitOfWork.Dispose();
            throw;
        }
    }

    public bool ValidateSchema(string schemaName)
    {
        // Verificar si existe el esquema
        return _factory.HasContext(schemaName);
    }
}
```

---

### **Ejemplo 2: Usar Factory con Genéricos (Type-Safe)**

Proporciona seguridad de tipos en tiempo de compilación:

```csharp
public class TypedMultiSchemaService
{
    private readonly IUnitOfWorkFactory _factory;

    public TypedMultiSchemaService(IUnitOfWorkFactory factory)
    {
        _factory = factory;
    }

    public void CreateUserAndProduct(string username, string productName)
    {
        // Obtener UnitOfWork tipados
        var accountUoW = _factory.GetUnitOfWork<AccountContext>();
        var catalogUoW = _factory.GetUnitOfWork<CatalogContext>();

        accountUoW.OpenTransaction();
        catalogUoW.OpenTransaction();

        try
        {
            // Crear usuario en esquema "account"
            var userRepo = accountUoW.GetRepository<User>();
            userRepo.Insert(new User { Username = username });

            // Crear producto en esquema "catalog"
            var productRepo = catalogUoW.GetRepository<Product>();
            productRepo.Insert(new Product { Name = productName });

            // Commit ambas transacciones
            accountUoW.Commit();
            catalogUoW.Commit();
        }
        catch
        {
            accountUoW.Dispose(); // Rollback automático
            catalogUoW.Dispose(); // Rollback automático
            throw;
        }
    }
}
```

---

### **Ejemplo 3: Servicio Multi-Esquema Complejo**

Trabajando con múltiples esquemas en una operación:

```csharp
public class OrderService
{
    private readonly IUnitOfWorkFactory _factory;

    public OrderService(IUnitOfWorkFactory factory)
    {
        _factory = factory;
    }

    public async Task<InvoiceDto> CreateOrder(CreateOrderDto dto)
    {
        var catalogUoW = _factory.GetUnitOfWork("catalog");
        var paymentUoW = _factory.GetUnitOfWork("payment");

        try
        {
            // 1. Validar producto existe en catálogo
            var productRepo = catalogUoW.GetRepository<Product>();
            var product = productRepo.Find(p => p.Id == dto.ProductId);

            if (product == null)
                throw new Exception("Producto no encontrado");

            // 2. Crear factura en esquema payment
            paymentUoW.OpenTransaction();

            var invoiceRepo = paymentUoW.GetRepository<Invoice>();
            var invoice = new Invoice
            {
                ProductId = dto.ProductId,
                Amount = product.Price * dto.Quantity,
                Date = DateTime.UtcNow,
                Status = "Pending"
            };

            invoiceRepo.Insert(invoice);
            paymentUoW.Commit();

            return MapToDto(invoice);
        }
        catch
        {
            paymentUoW.Dispose();
            throw;
        }
    }
}
```

---

### **Ejemplo 4: Inyección Tradicional (Sin Factory)**

El código tradicional **sigue funcionando** (backward compatible):

```csharp
public class TraditionalUserService
{
    private readonly IUnitOfWork<AccountContext> _accountUoW;

    // Inyección directa del contexto específico
    public TraditionalUserService(IUnitOfWork<AccountContext> accountUoW)
    {
        _accountUoW = accountUoW;
    }

    public User CreateUser(User user)
    {
        _accountUoW.OpenTransaction();
        try
        {
            var repo = _accountUoW.GetRepository<User>();
            var newUser = repo.Insert(user);
            _accountUoW.Commit();
            return newUser;
        }
        catch
        {
            _accountUoW.Dispose();
            throw;
        }
    }
}
```

---

## 🎨 Casos de Uso Comunes

### **Caso 1: Multi-Tenant con Esquemas por Cliente**

```csharp
public class TenantService
{
    private readonly IUnitOfWorkFactory _factory;

    public TenantService(IUnitOfWorkFactory factory)
    {
        _factory = factory;
    }

    public List<Customer> GetCustomersByTenant(string tenantId)
    {
        // tenant1 → esquema "tenant1"
        // tenant2 → esquema "tenant2"
        var tenantKey = $"tenant{tenantId}";

        if (!_factory.HasContext(tenantKey))
            throw new Exception($"Tenant {tenantId} no encontrado");

        var uow = _factory.GetUnitOfWork(tenantKey);
        var repo = uow.GetRepository<Customer>();

        return repo.Get(
            orderBy: q => q.OrderBy(c => c.Name),
            index: 0,
            size: 100
        ).Items.ToList();
    }
}
```

---

### **Caso 2: Importación de Datos entre Esquemas**

```csharp
public class DataMigrationService
{
    private readonly IUnitOfWorkFactory _factory;

    public DataMigrationService(IUnitOfWorkFactory factory)
    {
        _factory = factory;
    }

    public void MigrateUsersToNewSchema()
    {
        var oldUoW = _factory.GetUnitOfWork("account_old");
        var newUoW = _factory.GetUnitOfWork("account_new");

        var oldRepo = oldUoW.GetRepository<User>();
        var newRepo = newUoW.GetRepository<User>();

        // Leer todos los usuarios del esquema antiguo
        var users = oldRepo.Get(size: int.MaxValue).Items;

        newUoW.OpenTransaction();
        try
        {
            // Insertar en el nuevo esquema
            newRepo.InsertRange(users.ToList());
            newUoW.Commit();
        }
        catch
        {
            newUoW.Dispose();
            throw;
        }
    }
}
```

---

### **Caso 3: Reporting Multi-Esquema**

```csharp
public class ReportService
{
    private readonly IUnitOfWorkFactory _factory;

    public ReportService(IUnitOfWorkFactory factory)
    {
        _factory = factory;
    }

    public SalesReportDto GenerateSalesReport(DateTime from, DateTime to)
    {
        var catalogUoW = _factory.GetUnitOfWork<CatalogContext>();
        var paymentUoW = _factory.GetUnitOfWork<PaymentContext>();

        var productRepo = catalogUoW.GetRepository<Product>();
        var invoiceRepo = paymentUoW.GetRepository<Invoice>();

        // Obtener productos
        var products = productRepo.Get().Items;

        // Obtener facturas del período
        var invoices = invoiceRepo.Get(
            predicate: i => i.Date >= from && i.Date <= to
        ).Items;

        // Generar reporte combinando datos de ambos esquemas
        return new SalesReportDto
        {
            TotalProducts = products.Count,
            TotalInvoices = invoices.Count,
            TotalRevenue = invoices.Sum(i => i.Amount)
        };
    }
}
```

---

## 📚 API Reference

### **InjectUnitOfWorkExtension**

#### `AddUnitOfWork<TContext>()`
Registra un UnitOfWork sin clave (método tradicional).

```csharp
services.AddUnitOfWork<AccountContext>();
```

---

#### `AddUnitOfWork<TContext>(string contextKey)`
Registra un UnitOfWork con clave única para resolución dinámica.

**Parámetros:**
- `contextKey`: Clave única (ej: "account", "catalog")

**Excepciones:**
- `ArgumentNullException`: Si contextKey es null/vacío
- `InvalidOperationException`: Si la clave ya existe

```csharp
services.AddUnitOfWork<AccountContext>("account");
services.AddUnitOfWork<CatalogContext>("catalog");
```

---

#### `AddUnitOfWorkFactory()`
Registra la factory para resolución dinámica. Debe llamarse **después** de registrar todos los contextos.

```csharp
services.AddUnitOfWorkFactory();
```

---

#### `GetRegisteredContexts()`
Obtiene el diccionario de contextos registrados (útil para debugging).

```csharp
var contexts = InjectUnitOfWorkExtension.GetRegisteredContexts();
foreach (var kvp in contexts)
{
    Console.WriteLine($"{kvp.Key} → {kvp.Value.Name}");
}
// Output:
// account → AccountContext
// catalog → CatalogContext
// payment → PaymentContext
```

---

### **IUnitOfWorkFactory**

#### `GetUnitOfWork(string contextKey)`
Obtiene un UnitOfWork por clave.

**Parámetros:**
- `contextKey`: Clave del contexto

**Retorna:** `IUnitOfWork`

**Excepciones:**
- `ArgumentNullException`: Si contextKey es null/vacío
- `InvalidOperationException`: Si no existe el contexto

```csharp
var uow = factory.GetUnitOfWork("account");
```

---

#### `GetUnitOfWork<TContext>()`
Obtiene un UnitOfWork tipado genéricamente.

**Retorna:** `IUnitOfWork<TContext>`

**Excepciones:**
- `InvalidOperationException`: Si no se ha registrado el contexto

```csharp
var accountUoW = factory.GetUnitOfWork<AccountContext>();
```

---

#### `HasContext(string contextKey)`
Verifica si existe un contexto registrado.

**Parámetros:**
- `contextKey`: Clave a verificar

**Retorna:** `bool`

```csharp
if (factory.HasContext("account"))
{
    // Procesar...
}
```

---

## 🗄️ Estructura de Base de Datos

### **SQL Server - Crear Esquemas**

```sql
USE MiBaseDatos;
GO

-- Crear esquemas
CREATE SCHEMA account;
CREATE SCHEMA catalog;
CREATE SCHEMA payment;
GO

-- Tablas en esquema "account"
CREATE TABLE account.Users (
    Id INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100)
);

CREATE TABLE account.Roles (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(50) NOT NULL
);

-- Tablas en esquema "catalog"
CREATE TABLE catalog.Products (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(200) NOT NULL,
    Price DECIMAL(18,2)
);

CREATE TABLE catalog.Categories (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL
);

-- Tablas en esquema "payment"
CREATE TABLE payment.Invoices (
    Id INT PRIMARY KEY IDENTITY,
    ProductId INT,
    Amount DECIMAL(18,2),
    Date DATETIME,
    Status NVARCHAR(50)
);

CREATE TABLE payment.Transactions (
    Id INT PRIMARY KEY IDENTITY,
    InvoiceId INT,
    Status NVARCHAR(50)
);
```

---

### **Entity Framework Migrations**

Cada contexto necesita sus propias migraciones:

```bash
# AccountContext
dotnet ef migrations add InitialAccount \
    --context AccountContext \
    --output-dir Migrations/Account

# CatalogContext
dotnet ef migrations add InitialCatalog \
    --context CatalogContext \
    --output-dir Migrations/Catalog

# PaymentContext
dotnet ef migrations add InitialPayment \
    --context PaymentContext \
    --output-dir Migrations/Payment

# Aplicar todas las migraciones
dotnet ef database update --context AccountContext
dotnet ef database update --context CatalogContext
dotnet ef database update --context PaymentContext
```

---

## ⚠️ Consideraciones Importantes

### **1. Thread Safety**
El registro de contextos (`_contextRegistry`) está protegido con `lock` para evitar condiciones de carrera.

### **2. Scope de Servicios**
- **Factory:** `Scoped` (una instancia por HTTP request)
- **UnitOfWork:** `Scoped` (una instancia por HTTP request)
- **Registro de contextos:** `Singleton` (compartido en toda la app)

### **3. Transacciones**
Cada UnitOfWork maneja su propia transacción. Para transacciones distribuidas entre esquemas, considera usar `TransactionScope`.

### **4. Orden de Registro**
```csharp
// ✅ Correcto
services.AddUnitOfWork<AccountContext>("account");
services.AddUnitOfWork<CatalogContext>("catalog");
services.AddUnitOfWorkFactory(); // Al final

// ❌ Incorrecto
services.AddUnitOfWorkFactory(); // No hacer esto primero
services.AddUnitOfWork<AccountContext>("account");
```

---

## 🎯 Ventajas vs Desventajas

### **✅ Ventajas**
- Flexibilidad total en el proyecto consumidor
- Sin contextos hardcodeados en la librería
- Backward compatible con código existente
- Type-safe con genéricos
- Resolución dinámica en runtime
- Ideal para arquitectura modular

### **⚠️ Consideraciones**
- Requiere configuración explícita de claves
- Más complejidad inicial vs inyección directa simple
- Necesitas documentar las claves usadas en tu proyecto

---

## 📖 Recursos Adicionales

- [README.md](README.md) - Documentación principal
- [Unit of Work Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [Repository Pattern - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

---

## 🤝 Contribuciones

Si encuentras bugs o tienes sugerencias, abre un issue en el repositorio.

---

**¡Disfruta del poder de múltiples esquemas con Factory Pattern!** 🚀
