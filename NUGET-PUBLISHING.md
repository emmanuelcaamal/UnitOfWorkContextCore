# 📦 Guía de Publicación en NuGet

Esta guía te ayudará a publicar los paquetes **UnitOfWorkContext.Core** y **UnitOfWorkContext.DependencyInjection** en NuGet.org.

---

## 🎯 Paquetes a Publicar

| Paquete | ID | Versión | Archivo |
|---------|----|---------|---------|
| Core | `UnitOfWorkContext.Core` | 1.9.0 | UnitOfWorkContext.Core.1.9.0.nupkg |
| Dependency Injection | `UnitOfWorkContext.DependencyInjection` | 1.9.0 | UnitOfWorkContext.DependencyInjection.1.9.0.nupkg |

---

## 📋 Pre-requisitos

1. **Cuenta en NuGet.org**
   - Si no tienes cuenta: https://www.nuget.org/users/account/LogOn

2. **API Key de NuGet**
   - Ve a: https://www.nuget.org/account/apikeys
   - Click en "Create"
   - Nombre: `UnitOfWorkContext-Publishing`
   - Glob Pattern: `UnitOfWorkContext.*`
   - Expiration: 365 días
   - Scopes: `Push new packages and package versions`
   - **Guarda tu API Key** (solo se muestra una vez)

3. **.NET CLI**
   ```bash
   dotnet --version  # Debe ser 8.0 o superior
   ```

---

## 🚀 Pasos para Publicar

### **Método 1: Usando .NET CLI (Recomendado)**

#### **Paso 1: Limpiar y Compilar**

```bash
# Navegar al directorio del proyecto
cd /Users/emmanuelcaamal/Projects/UnitOfWorkContextCore

# Limpiar builds anteriores
dotnet clean

# Compilar en Release mode (genera los .nupkg)
dotnet build -c Release
```

Los archivos `.nupkg` se generarán en:
- `UnitOfWorkContextCore/bin/Release/UnitOfWorkContext.Core.1.9.0.nupkg`
- `UnitOfWorkContextCore.DependencyInjection/bin/Release/UnitOfWorkContext.DependencyInjection.1.9.0.nupkg`

#### **Paso 2: Configurar API Key**

```bash
# Configurar tu API key (solo una vez)
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org

# O si ya existe:
dotnet nuget update source nuget.org -s https://api.nuget.org/v3/index.json

# Almacenar API key
dotnet nuget setApiKey TU-API-KEY-AQUI -Source nuget.org
```

#### **Paso 3: Publicar UnitOfWorkContext.Core**

```bash
dotnet nuget push \
  UnitOfWorkContextCore/bin/Release/UnitOfWorkContext.Core.1.9.0.nupkg \
  --source nuget.org \
  --api-key TU-API-KEY-AQUI
```

Respuesta esperada:
```
Pushing UnitOfWorkContext.Core.1.9.0.nupkg to 'https://www.nuget.org/api/v2/package'...
  PUT https://www.nuget.org/api/v2/package/
  Created https://www.nuget.org/api/v2/package/ 3825ms
Your package was pushed.
```

#### **Paso 4: Publicar UnitOfWorkContext.DependencyInjection**

```bash
dotnet nuget push \
  UnitOfWorkContextCore.DependencyInjection/bin/Release/UnitOfWorkContext.DependencyInjection.1.9.0.nupkg \
  --source nuget.org \
  --api-key TU-API-KEY-AQUI
```

---

### **Método 2: Usando NuGet.org Web UI**

#### **Paso 1: Generar los paquetes**

```bash
cd /Users/emmanuelcaamal/Projects/UnitOfWorkContextCore
dotnet build -c Release
```

#### **Paso 2: Subir manualmente**

1. Ve a https://www.nuget.org/packages/manage/upload
2. Click en "Browse" y selecciona el archivo `.nupkg`
3. Sube primero `UnitOfWorkContext.Core.1.9.0.nupkg`
4. Click en "Verify Details" → "Submit"
5. Repite para `UnitOfWorkContext.DependencyInjection.1.9.0.nupkg`

---

## ✅ Verificación Post-Publicación

### **1. Espera la Indexación (5-10 minutos)**

NuGet.org tarda unos minutos en indexar el paquete.

### **2. Verifica en NuGet.org**

- Core: https://www.nuget.org/packages/UnitOfWorkContext.Core/
- DI: https://www.nuget.org/packages/UnitOfWorkContext.DependencyInjection/

### **3. Prueba la Instalación**

```bash
# Crear proyecto de prueba
dotnet new console -n TestNuGet
cd TestNuGet

# Instalar paquetes
dotnet add package UnitOfWorkContext.Core --version 1.9.0
dotnet add package UnitOfWorkContext.DependencyInjection --version 1.9.0

# Verificar instalación
dotnet list package
```

Deberías ver:
```
Project 'TestNuGet' has the following package references
   [net8.0]:
   Top-level Package                                  Requested   Resolved
   > UnitOfWorkContext.Core                           1.9.0       1.9.0
   > UnitOfWorkContext.DependencyInjection            1.9.0       1.9.0
```

---

## 📝 Checklist Pre-Publicación

Antes de publicar, verifica que:

- [ ] La versión en ambos `.csproj` es correcta (1.9.0)
- [ ] El README.md está actualizado con la nueva versión
- [ ] El CHANGELOG.md refleja los cambios de v1.9.0
- [ ] Los Release Notes en `.csproj` están actualizados
- [ ] La compilación es exitosa: `dotnet build -c Release`
- [ ] Los tests pasan (si existen)
- [ ] Los paquetes `.nupkg` se generaron correctamente
- [ ] Tienes tu API Key de NuGet lista

---

## 🔄 Publicar Nuevas Versiones

### **Actualizar Versión**

1. Edita ambos archivos `.csproj`:
   ```xml
   <Version>1.8.1</Version>
   <PackageVersion>1.8.1</PackageVersion>
   ```

2. Actualiza `CHANGELOG.md`:
   ```markdown
   ## [1.8.1] - 2024-XX-XX
   ### Corregido
   - Fix: ...
   ```

3. Actualiza `PackageReleaseNotes` en `.csproj`

4. Compila y publica:
   ```bash
   dotnet clean
   dotnet build -c Release
   dotnet nuget push "**/*.nupkg" --source nuget.org --skip-duplicate
   ```

---

## 🛠️ Comandos Útiles

### **Ver todas las versiones publicadas**
```bash
dotnet nuget list UnitOfWorkContext.Core --source nuget.org
```

### **Eliminar versión (dentro de 72 horas)**
```bash
dotnet nuget delete UnitOfWorkContext.Core 1.9.0 \
  --source nuget.org \
  --api-key TU-API-KEY
```

### **Publicar múltiples paquetes a la vez**
```bash
dotnet nuget push "**/*.nupkg" \
  --source nuget.org \
  --api-key TU-API-KEY \
  --skip-duplicate
```

### **Validar paquete antes de publicar**
```bash
dotnet nuget verify UnitOfWorkContextCore/bin/Release/*.nupkg
```

---

## 📊 Estadísticas Post-Publicación

Después de publicar, podrás ver:

- **Descargas:** https://www.nuget.org/stats/packages/UnitOfWorkContext.Core
- **Versiones:** https://www.nuget.org/packages/UnitOfWorkContext.Core#versions-body-tab
- **Dependencies:** https://www.nuget.org/packages/UnitOfWorkContext.Core#dependencies-body-tab

---

## 🔐 Seguridad de API Key

⚠️ **NUNCA** publiques tu API Key en:
- Código fuente
- Repositorios Git
- Issues o Pull Requests
- Documentación pública

**Buenas prácticas:**
- Usa variables de entorno: `$env:NUGET_API_KEY`
- Rota la key periódicamente
- Usa scopes limitados (solo push, no delete)
- Establece fecha de expiración

---

## 🆘 Solución de Problemas

### **Error: "Package already exists"**

```
Error: Response status code does not indicate success: 409 (Conflict -
The package already exists.).
```

**Solución:** Incrementa la versión en `.csproj` y recompila.

---

### **Error: "Invalid API Key"**

```
error: Response status code does not indicate success: 403 (Forbidden).
```

**Soluciones:**
1. Verifica que la API key sea correcta
2. Verifica que la key no haya expirado
3. Verifica que tenga permisos de "Push"

---

### **Error: "Package validation failed"**

```
Error: Package validation failed.
```

**Soluciones:**
1. Verifica que el README.md exista en la ruta especificada
2. Compila en Release: `dotnet build -c Release`
3. Verifica metadata en `.csproj`

---

## 📖 Recursos Adicionales

- [Documentación oficial de NuGet](https://docs.microsoft.com/nuget/)
- [Mejores prácticas de paquetes](https://docs.microsoft.com/nuget/create-packages/package-authoring-best-practices)
- [Versionamiento Semántico](https://semver.org/)
- [Licencias de código abierto](https://choosealicense.com/)

---

## ✅ Checklist Final

Antes de considerar la publicación completa:

- [ ] Ambos paquetes publicados exitosamente
- [ ] Los paquetes aparecen en NuGet.org (espera 5-10 min)
- [ ] El README se muestra correctamente en NuGet.org
- [ ] Los Release Notes son correctos
- [ ] Las dependencias se resuelven correctamente
- [ ] Probaste instalar desde NuGet en un proyecto nuevo
- [ ] Etiquetaste la versión en Git: `git tag v1.9.0`
- [ ] Hiciste push del tag: `git push origin v1.9.0`

---

**¡Listo para publicar!** 🎉

Para cualquier duda, consulta la [documentación oficial de NuGet](https://docs.microsoft.com/nuget/).
