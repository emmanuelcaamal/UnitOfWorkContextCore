# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

UnitOfWorkContextCore is a .NET library that abstracts database transaction management using the Unit of Work pattern with Entity Framework Core 8. It's published as a NuGet package (UnitOfWorkContext.Core) and provides a clean way to manage database operations and transactions.

## Build and Development Commands

### Building the Solution
```bash
dotnet build UnitOfWorkContext.sln
```

### Building Specific Projects
```bash
# Build core library
dotnet build UnitOfWorkContextCore/UnitOfWorkContextCore.csproj

# Build dependency injection extension
dotnet build UnitOfWorkContextCore.DependencyInjection/UnitOfWorkContextCore.DependencyInjection.csproj

# Build workspace context
dotnet build UnitOfWorkContextCore.WorkspaceContext/UnitOfWorkContextCore.WorkspaceContext.csproj
```

### Packaging
```bash
# Generate NuGet package (enabled by default via GeneratePackageOnBuild)
dotnet pack UnitOfWorkContextCore/UnitOfWorkContextCore.csproj
```

### Restore Dependencies
```bash
dotnet restore UnitOfWorkContext.sln
```

## Architecture

### Core Components

The library consists of three main projects:

1. **UnitOfWorkContextCore** - Core implementation
   - Package ID: `UnitOfWorkContext.Core`
   - Current Version: 1.8.0.13
   - Target Framework: .NET 8.0
   - Depends on: Entity Framework Core 8.0.13

2. **UnitOfWorkContextCore.DependencyInjection** - DI extensions for ASP.NET Core

3. **UnitOfWorkContextCore.WorkspaceContext** - Workspace-specific DbContext implementation

### Key Patterns

#### Unit of Work Pattern
The library implements two variants of the Unit of Work pattern:

- **`IUnitOfWork<TContext>`**: Generic implementation that works with any DbContext
  - Location: `UnitOfWorkContextCore/Interfaces/IUnitOfWork.cs`
  - Implementation: `UnitOfWorkContextCore/UnitOfWork.cs:9`
  - Manages transaction lifecycle: `OpenTransaction()`, `Commit()`, and rollback on exceptions
  - Provides repository caching to avoid multiple instances for the same entity type

- **`IUnitOfWorkspace`**: Specialized implementation for workspace scenarios
  - Location: `UnitOfWorkContextCore/Interfaces/IUnitOfWorkspace.cs`
  - Implementation: `UnitOfWorkContextCore/UnitOfWorkspace.cs:10`
  - Uses thread-static DbContext (line 14)
  - Works with `WorkspaceDbContext` base class

#### Repository Pattern
Two-tier repository structure:

- **`IReadRepository<T>`**: Read-only operations
  - Location: `UnitOfWorkContextCore/Interfaces/IReadRepository.cs`
  - Provides `Find()` and `Get()` methods with pagination support
  - Supports Entity Framework Include/OrderBy expressions
  - Configurable change tracking via `enableTracking` parameter

- **`IRepository<T>`**: Full CRUD operations (extends IReadRepository)
  - Location: `UnitOfWorkContextCore/Interfaces/IRepository.cs`
  - Adds: `Insert()`, `Update()`, `Remove()` and their batch variants
  - Implementation: `UnitOfWorkContextCore/Repository.cs:7`

#### Repository Caching
Both UnitOfWork implementations cache repository instances using a dictionary keyed by `(Type type, string name)` to ensure only one repository instance per entity type exists per unit of work (see `GetGenericRepository()` in `UnitOfWork.cs:69` and `UnitOfWorkspace.cs:74`).

### Transaction Management

Transactions are managed explicitly:
- `OpenTransaction()`: Begins a new database transaction if one doesn't exist
- `Commit()`: Saves changes and commits transaction, rolling back on exceptions
- Automatic cleanup: Disposes transaction resources after commit/rollback
- Exception handling: Rethrows original exception after rollback

### Pagination System

Built-in pagination support via `IPaginate<T>` interface:
- Location: `UnitOfWorkContextCore/Interfaces/Paging/`
- Extension method: `ToPaginate()` in `PaginateExtensions.cs`
- All `Get()` methods support pagination with `index` and `size` parameters
- Also includes DataTable extensions for compatibility

### Dependency Injection

Register the Unit of Work pattern:
```csharp
services.AddUnitOfWork<YourDbContext>();
```

This registers both `IUnitOfWork` and `IUnitOfWork<TContext>` as scoped services.
Location: `UnitOfWorkContextCore.DependencyInjection/InjectUnitOfWorkExtension.cs:9`

### WorkspaceDbContext

Abstract base class for workspace-specific contexts:
- Location: `UnitOfWorkContextCore.WorkspaceContext/WorkspaceDbContext.cs:6`
- Preconfigured for SQL Server
- Accepts connection string or DbContextOptions
- Auto-applies entity configurations from assembly via `ApplyConfigurationsFromAssembly()`

## Important Implementation Details

### Change Tracking
All repository query methods accept an `enableTracking` parameter (defaults to `true`). Set to `false` for read-only scenarios to improve performance by using `AsNoTracking()`.

### Workspace vs Regular UnitOfWork
- `UnitOfWork<TContext>`: Use for standard scenarios with dependency injection
- `UnitOfWorkspace`: Use for multi-tenant/workspace scenarios where context is thread-specific

### Repository Instance Management
Repositories are cached per unit of work instance. The caching key includes both the entity type AND the repository's full type name, allowing for custom repository implementations alongside generic ones.

## Git Workflow

Current branch: `EntityFramework8`
Main branch: `master`

Recent upgrade: Migrated to Entity Framework Core 8.0.13
