# 🚀 Inicio Rápido - Múltiples Esquemas

Esta guía te permite implementar **múltiples esquemas en 5 minutos**. Copia y pega el código en tu proyecto.

---

## 📦 Instalación

```bash
dotnet add package UnitOfWorkContext.Core
dotnet add package UnitOfWorkContext.DependencyInjection
```

---

## 🎯 Implementación en 3 Pasos

### **Paso 1: Crear tus DbContext**

Crea un archivo por cada contexto con su esquema:

#### `Data/AccountContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using TuProyecto.Models;

namespace TuProyecto.Data
{
    public class AccountContext : DbContext
    {
        public AccountContext(DbContextOptions<AccountContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplicar esquema "account" a todas las tablas
            modelBuilder.HasDefaultSchema("account");

            base.OnModelCreating(modelBuilder);
        }
    }
}
```

#### `Data/CatalogContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using TuProyecto.Models;

namespace TuProyecto.Data
{
    public class CatalogContext : DbContext
    {
        public CatalogContext(DbContextOptions<CatalogContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplicar esquema "catalog" a todas las tablas
            modelBuilder.HasDefaultSchema("catalog");

            base.OnModelCreating(modelBuilder);
        }
    }
}
```

#### `Data/PaymentContext.cs`
```csharp
using Microsoft.EntityFrameworkCore;
using TuProyecto.Models;

namespace TuProyecto.Data
{
    public class PaymentContext : DbContext
    {
        public PaymentContext(DbContextOptions<PaymentContext> options)
            : base(options) { }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplicar esquema "payment" a todas las tablas
            modelBuilder.HasDefaultSchema("payment");

            base.OnModelCreating(modelBuilder);
        }
    }
}
```

---

### **Paso 2: Configurar en Program.cs**

#### Para .NET 6+:

```csharp
using Microsoft.EntityFrameworkCore;
using UnitOfWorkContextCore.DependencyInjection;
using TuProyecto.Data;

var builder = WebApplication.CreateBuilder(args);

// Connection String (MISMA base de datos, DIFERENTES esquemas)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 1️⃣ Registrar los DbContext
builder.Services.AddDbContext<AccountContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<CatalogContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<PaymentContext>(options =>
    options.UseSqlServer(connectionString));

// 2️⃣ Registrar UnitOfWork con claves únicas
builder.Services.AddUnitOfWork<AccountContext>("account");
builder.Services.AddUnitOfWork<CatalogContext>("catalog");
builder.Services.AddUnitOfWork<PaymentContext>("payment");

// 3️⃣ Registrar la Factory
builder.Services.AddUnitOfWorkFactory();

// Registrar tus servicios
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
```

#### Para .NET 5 / .NET Core 3.1 (Startup.cs):

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnitOfWorkContextCore.DependencyInjection;
using TuProyecto.Data;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var connectionString = Configuration.GetConnectionString("DefaultConnection");

        // 1️⃣ Registrar los DbContext
        services.AddDbContext<AccountContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddDbContext<CatalogContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddDbContext<PaymentContext>(options =>
            options.UseSqlServer(connectionString));

        // 2️⃣ Registrar UnitOfWork con claves únicas
        services.AddUnitOfWork<AccountContext>("account");
        services.AddUnitOfWork<CatalogContext>("catalog");
        services.AddUnitOfWork<PaymentContext>("payment");

        // 3️⃣ Registrar la Factory
        services.AddUnitOfWorkFactory();

        // Tus servicios
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
```

#### `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MiBaseDatos;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

---

### **Paso 3: Usar en tus Servicios**

#### Opción A: Con Factory (Recomendado para múltiples esquemas)

```csharp
using UnitOfWorkContextCore.Interfaces;
using TuProyecto.Models;

namespace TuProyecto.Services
{
    public interface IUserService
    {
        User CreateUser(string username, string email);
        List<User> GetAllUsers();
    }

    public class UserService : IUserService
    {
        private readonly IUnitOfWorkFactory _factory;

        public UserService(IUnitOfWorkFactory factory)
        {
            _factory = factory;
        }

        public User CreateUser(string username, string email)
        {
            // Obtener UnitOfWork del esquema "account"
            var accountUoW = _factory.GetUnitOfWork("account");

            accountUoW.OpenTransaction();
            try
            {
                var userRepo = accountUoW.GetRepository<User>();
                var user = new User
                {
                    Username = username,
                    Email = email,
                    CreatedAt = DateTime.UtcNow
                };

                userRepo.Insert(user);
                accountUoW.Commit();

                return user;
            }
            catch
            {
                accountUoW.Dispose(); // Rollback automático
                throw;
            }
        }

        public List<User> GetAllUsers()
        {
            var accountUoW = _factory.GetUnitOfWork("account");
            var userRepo = accountUoW.GetRepository<User>();

            return userRepo.Get(
                orderBy: q => q.OrderBy(u => u.Username),
                size: 100
            ).Items.ToList();
        }
    }
}
```

#### Opción B: Con Type-Safe Generics

```csharp
using UnitOfWorkContextCore.Interfaces;
using TuProyecto.Data;
using TuProyecto.Models;

namespace TuProyecto.Services
{
    public interface IProductService
    {
        Product CreateProduct(string name, decimal price);
        IPaginate<Product> GetProducts(int page);
    }

    public class ProductService : IProductService
    {
        private readonly IUnitOfWorkFactory _factory;

        public ProductService(IUnitOfWorkFactory factory)
        {
            _factory = factory;
        }

        public Product CreateProduct(string name, decimal price)
        {
            // Obtener UnitOfWork tipado (type-safe)
            var catalogUoW = _factory.GetUnitOfWork<CatalogContext>();

            catalogUoW.OpenTransaction();
            try
            {
                var productRepo = catalogUoW.GetRepository<Product>();
                var product = new Product
                {
                    Name = name,
                    Price = price,
                    CreatedAt = DateTime.UtcNow
                };

                productRepo.Insert(product);
                catalogUoW.Commit();

                return product;
            }
            catch
            {
                catalogUoW.Dispose();
                throw;
            }
        }

        public IPaginate<Product> GetProducts(int page)
        {
            var catalogUoW = _factory.GetUnitOfWork<CatalogContext>();
            var productRepo = catalogUoW.GetRepository<Product>();

            return productRepo.Get(
                predicate: p => p.Price > 0,
                orderBy: q => q.OrderByDescending(p => p.CreatedAt),
                index: page - 1,
                size: 20
            );
        }
    }
}
```

#### Opción C: Servicio Multi-Esquema

```csharp
using UnitOfWorkContextCore.Interfaces;
using TuProyecto.Data;
using TuProyecto.Models;

namespace TuProyecto.Services
{
    public interface IOrderService
    {
        Invoice CreateOrder(int userId, int productId, int quantity);
    }

    public class OrderService : IOrderService
    {
        private readonly IUnitOfWorkFactory _factory;

        public OrderService(IUnitOfWorkFactory factory)
        {
            _factory = factory;
        }

        public Invoice CreateOrder(int userId, int productId, int quantity)
        {
            // Trabajar con MÚLTIPLES esquemas en una operación
            var accountUoW = _factory.GetUnitOfWork<AccountContext>();
            var catalogUoW = _factory.GetUnitOfWork<CatalogContext>();
            var paymentUoW = _factory.GetUnitOfWork<PaymentContext>();

            try
            {
                // 1. Validar usuario existe (esquema "account")
                var userRepo = accountUoW.GetRepository<User>();
                var user = userRepo.Find(u => u.Id == userId);
                if (user == null)
                    throw new Exception("Usuario no encontrado");

                // 2. Validar producto existe y obtener precio (esquema "catalog")
                var productRepo = catalogUoW.GetRepository<Product>();
                var product = productRepo.Find(p => p.Id == productId);
                if (product == null)
                    throw new Exception("Producto no encontrado");

                // 3. Crear factura (esquema "payment")
                paymentUoW.OpenTransaction();

                var invoiceRepo = paymentUoW.GetRepository<Invoice>();
                var invoice = new Invoice
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    Amount = product.Price * quantity,
                    Date = DateTime.UtcNow,
                    Status = "Pending"
                };

                invoiceRepo.Insert(invoice);
                paymentUoW.Commit();

                return invoice;
            }
            catch
            {
                paymentUoW.Dispose();
                throw;
            }
        }
    }
}
```

---

## 🎮 Usar en Controladores

```csharp
using Microsoft.AspNetCore.Mvc;
using TuProyecto.Services;
using TuProyecto.Models;

namespace TuProyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] CreateUserDto dto)
        {
            var user = _userService.CreateUser(dto.Username, dto.Email);
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _userService.GetAllUsers();
            return Ok(users);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public IActionResult CreateProduct([FromBody] CreateProductDto dto)
        {
            var product = _productService.CreateProduct(dto.Name, dto.Price);
            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }

        [HttpGet]
        public IActionResult GetProducts([FromQuery] int page = 1)
        {
            var products = _productService.GetProducts(page);
            return Ok(new
            {
                items = products.Items,
                totalPages = products.Pages,
                currentPage = page
            });
        }
    }
}
```

---

## 🗄️ Crear Esquemas en SQL Server

Ejecuta este script en tu base de datos:

```sql
USE MiBaseDatos;
GO

-- Crear esquemas
CREATE SCHEMA account;
CREATE SCHEMA catalog;
CREATE SCHEMA payment;
GO

-- Verificar esquemas creados
SELECT name FROM sys.schemas WHERE name IN ('account', 'catalog', 'payment');
GO
```

---

## 🔄 Migraciones de Entity Framework

```bash
# Crear migraciones para cada contexto
dotnet ef migrations add InitialAccount --context AccountContext --output-dir Migrations/Account
dotnet ef migrations add InitialCatalog --context CatalogContext --output-dir Migrations/Catalog
dotnet ef migrations add InitialPayment --context PaymentContext --output-dir Migrations/Payment

# Aplicar migraciones
dotnet ef database update --context AccountContext
dotnet ef database update --context CatalogContext
dotnet ef database update --context PaymentContext
```

---

## ✅ Verificar Instalación

Crea un endpoint de prueba:

```csharp
[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IUnitOfWorkFactory _factory;

    public DiagnosticsController(IUnitOfWorkFactory factory)
    {
        _factory = factory;
    }

    [HttpGet("schemas")]
    public IActionResult GetRegisteredSchemas()
    {
        var contexts = InjectUnitOfWorkExtension.GetRegisteredContexts();

        return Ok(new
        {
            totalSchemas = contexts.Count,
            schemas = contexts.Select(kvp => new
            {
                key = kvp.Key,
                contextType = kvp.Value.Name,
                isRegistered = _factory.HasContext(kvp.Key)
            })
        });
    }
}
```

Ejecuta tu API y visita: `GET /api/diagnostics/schemas`

**Respuesta esperada:**
```json
{
  "totalSchemas": 3,
  "schemas": [
    {
      "key": "account",
      "contextType": "AccountContext",
      "isRegistered": true
    },
    {
      "key": "catalog",
      "contextType": "CatalogContext",
      "isRegistered": true
    },
    {
      "key": "payment",
      "contextType": "PaymentContext",
      "isRegistered": true
    }
  ]
}
```

---

## 🎯 Próximos Pasos

1. ✅ Lee la [documentación completa](README-MultiSchema.md)
2. ✅ Explora [ejemplos avanzados](README-MultiSchema.md#casos-de-uso-comunes)
3. ✅ Implementa tus propios esquemas
4. ✅ Configura tus migraciones

---

## 🆘 Troubleshooting

### **Error: "No se encontró un contexto registrado con la clave 'account'"**

**Solución:** Asegúrate de haber registrado el contexto antes de la factory:

```csharp
// ✅ Correcto
builder.Services.AddUnitOfWork<AccountContext>("account");
builder.Services.AddUnitOfWorkFactory();

// ❌ Incorrecto
builder.Services.AddUnitOfWorkFactory();
builder.Services.AddUnitOfWork<AccountContext>("account"); // Demasiado tarde
```

---

### **Error: "Ya existe un contexto registrado con la clave 'account'"**

**Solución:** Las claves deben ser únicas. Usa claves diferentes:

```csharp
// ❌ Incorrecto
builder.Services.AddUnitOfWork<AccountContext>("account");
builder.Services.AddUnitOfWork<AccountV2Context>("account"); // Duplicado!

// ✅ Correcto
builder.Services.AddUnitOfWork<AccountContext>("account");
builder.Services.AddUnitOfWork<AccountV2Context>("account_v2");
```

---

### **Las tablas se crean en el esquema "dbo" en lugar del esquema especificado**

**Solución:** Asegúrate de llamar `modelBuilder.HasDefaultSchema()` en `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema("account"); // ← NO olvides esto
    base.OnModelCreating(modelBuilder);
}
```

---

**¡Listo! Ya tienes múltiples esquemas funcionando.** 🎉
