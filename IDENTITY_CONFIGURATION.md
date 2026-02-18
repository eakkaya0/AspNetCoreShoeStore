# 🔐 Guía de Configuración Identity Segura

Este documento explica cómo configurar y ejecutar el sistema Identity profesional en diferentes ambientes.

---

## 📋 Resumen de Cambios

### ✅ Mejoras Implementadas

1. **Seeding Idempotent**
   - Las operaciones de seed son seguras de ejecutar múltiples veces
   - No crea duplicados
   - Verifica roles y asignaciones existentes

2. **Environment-Aware**
   - Development: Auto-seed en startup
   - Production: No auto-seed (evita cambios inesperados)
   - Logging detallado de cada operación

3. **Seguridad de Contraseña**
   - Prioridad 1: Variables de entorno
   - Prioridad 2: User Secrets
   - Prioridad 3: appsettings (solo para fallback)
   - Nunca en control de versiones

4. **Calidad de Código**
   - Single Responsibility Principle
   - Métodos privados claros y específicos
   - Logging en cada operación
   - Manejo robusto de excepciones

5. **Dependency Injection**
   - IdentitySeeder es ahora un servicio registrado
   - Inyectado automáticamente
   - Acceso a logger incluido

---

## 🔧 Configuración por Ambiente

### DEVELOPMENT (Desarrollo Local)

#### Opción 1: User Secrets (RECOMENDADO)

```bash
# En la carpeta del proyecto ECommerceWeb
# Inicializar User Secrets (una sola vez)
dotnet user-secrets init

# Establecer la contraseña del admin
dotnet user-secrets set "AdminUser:Password" "Admin@12345"

# Verificar que se configuró correctamente
dotnet user-secrets list
```

**Ubicación de los User Secrets:**
```
Windows: %APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json
Mac:    ~/.microsoft/usersecrets/<user_secrets_id>/secrets.json
Linux:  ~/.microsoft/usersecrets/<user_secrets_id>/secrets.json
```

El `user_secrets_id` está en `ECommerceWeb.csproj`:
```xml
<PropertyGroup>
  <UserSecretsId>xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</UserSecretsId>
</PropertyGroup>
```

#### Opción 2: appsettings.Development.json

```json
{
  "AdminUser": {
    "Password": "Admin@12345"
  }
}
```

**Ventajas:**
- Simple para desarrollo
- Funciona sin configuración adicional

**Desventajas:**
- No es portable
- Fácil olvidar cambiar en production

**Recomendación:** Usar User Secrets en lugar de esto.

#### Opción 3: Variable de Entorno

```bash
# PowerShell
$env:ADMINUSER_PASSWORD = "Admin@12345"

# Command Prompt
set ADMINUSER_PASSWORD=Admin@12345

# Linux/Mac
export ADMINUSER_PASSWORD=Admin@12345
```

---

### PRODUCTION (Servidor)

#### ⚠️ IMPORTANTE: Nunca en appsettings.json

La contraseña debe SIEMPRE come de variables de entorno.

#### Opción 1: Environment Variables en IIS

En IIS Manager:
1. Seleccionar la aplicación
2. Application Pool → Advanced Settings
3. Set Environment Variables
4. Nombre: `ADMINUSER_PASSWORD`
5. Valor: Tu contraseña segura

#### Opción 2: Environment Variables del Sistema

**Windows Server:**
```powershell
# PowerShell (como Administrator)
[Environment]::SetEnvironmentVariable("ADMINUSER_PASSWORD", "YourSecurePassword", "Machine")

# Reiniciar la aplicación para que se aplique
```

**Linux:**
```bash
# En /etc/environment
ADMINUSER_PASSWORD=YourSecurePassword

# O en systemd service file
[Service]
Environment="ADMINUSER_PASSWORD=YourSecurePassword"
```

#### Opción 3: Azure Key Vault (Para aplicaciones Azure)

```csharp
// En Program.cs
var keyVaultEndpoint = new Uri(builder.Configuration["KeyVault:Uri"]);
builder.Configuration.AddAzureKeyVault(
    keyVaultEndpoint,
    new DefaultAzureCredential()
);
```

---

## 🚀 Ejecución de Seed

### Automático (Development)

El seed se ejecuta automáticamente al iniciar la aplicación en Development:

```bash
dotnet run
# INFO: Starting Identity seeding operation
# INFO: Development environment detected, auto-seeding Identity data
# INFO: Identity seeding completed successfully
```

### Manual (Production)

Para ejecutar seed manualmente en production, crear un endpoint administrativo:

```csharp
// Controllers/AdminController.cs (con autorización)
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IdentitySeeder _seeder;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IdentitySeeder seeder, ILogger<AdminController> logger)
    {
        _seeder = seeder;
        _logger = logger;
    }

    [HttpPost("seed-identity")]
    public async Task<IActionResult> SeedIdentity()
    {
        try
        {
            _logger.LogWarning("Starting manual Identity seed from endpoint");
            await _seeder.SeedAsync();
            return Ok("Identity seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Seed failed: {ex.Message}");
            return StatusCode(500, $"Seed failed: {ex.Message}");
        }
    }
}
```

**Uso:**
```bash
POST https://yourdomain.com/api/admin/seed-identity
Authorization: Bearer <admin_token>
```

---

## 📋 Checklist de Deployment

### Antes de Deploy a Production

- [ ] Cambiar `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Contraseña admin configurada en variables de entorno
- [ ] Connection string actualizado para base de datos production
- [ ] Email real del admin en appsettings.Production.json
- [ ] Logging configurado para Production (menos verbose)
- [ ] HTTPS habilitado
- [ ] Cookie settings configurado para dominio actual

### Después de Deploy

- [ ] Verificar que aplicación inicia sin errores de seed
- [ ] Logear con credenciales admin
- [ ] Verificar roles están asignados correctamente
- [ ] Si no hay admin inicial, ejecutar endpoint de seed manual

---

## 🔍 Troubleshooting

### Error: "AdminUser:Email is not configured"

**Solución:**
```bash
# Verificar que AdminUser:Email está en appsettings
# Debería ser: admin@ecommerce.com (por defecto)
```

### Error: "AdminUser:Password is not configured"

**Solución:**
```bash
# Development con User Secrets:
dotnet user-secrets set "AdminUser:Password" "admin@12345"

# Production con variable de entorno:
set ADMINUSER_PASSWORD=admin@12345

# O en appsettings.Development.json:
# {
#   "AdminUser": {
#     "Password": "admin@12345"
#   }
# }
```

### Seed se ejecuta en Production involuntariamente

**Verificar:**
```bash
# Asegurarse que ASPNETCORE_ENVIRONMENT=Production
echo %ASPNETCORE_ENVIRONMENT%

# Si está vacío, aplicación está en Development mode
```

### Admin no tiene rol Admin asignado

**Solución:**
```bash
# Ejecutar endpoint manual de seed (si existe)
POST /api/admin/seed-identity

# O check directamente en database:
SELECT u.Email, ur.RoleId
FROM AspNetUsers u
LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
WHERE u.Email = 'admin@ecommerce.com'
```

---

## 📊 Ejemplo: Flujo Completo Development

```bash
# 1. Clonar proyecto
git clone <repo>
cd ECommerceWeb

# 2. Inicializar User Secrets (primera vez)
dotnet user-secrets init

# 3. Configurar contraseña admin
dotnet user-secrets set "AdminUser:Password" "Admin@12345"

# 4. Restaurar packages
dotnet restore

# 5. Crear/Actualizar database
dotnet ef database update

# 6. Ejecutar aplicación (seed automático)
dotnet run

# 7. Logear con:
#    Email: admin@ecommerce.com
#    Password: Admin@12345
```

---

## 📊 Ejemplo: Flujo Completo Production (Azure)

```bash
# 1. Deploy aplicación
# (normal deployment process)

# 2. Configurar variables de entorno en App Service
# ASPNETCORE_ENVIRONMENT=Production
# ADMINUSER_PASSWORD=YourSecurePassword123!

# 3. Aplicar migraciones de base de datos
# (via DbUp, Entity Framework migrations, o manual)

# 4. Acceder a endpoint de seed (si usas endpoint manual)
# POST https://yourdomain.azurewebsites.net/api/admin/seed-identity

# 5. Verificar admin puede logear
```

---

## 🔒 Security Best Practices

1. **Nunca committear contraseñas**
   ```bash
   # .gitignore
   appsettings.*.json  # Pero mantener appsettings.json base
   secrets.json
   ```

2. **Usar diferentes contraseñas por ambiente**
   - Development: Admin@12345
   - Staging: SecureStaging123!$#@
   - Production: VerySecureProduction!$#@xyz

3. **Rotar contraseñas regularmente**
   - Cambiar en Variables de Entorno
   - No requiere redeploy

4. **Auditar login attempts**
   ```csharp
   // En AspNetCore.Identity logs
   // Check ASPNETCORE_ENVIRONMENT=Development logs
   ```

5. **Usar HTTPS en production**
   - Obligatorio para cookies de autenticación
   - Ya está habilitado en Program.cs

---

## 📚 Referencia: Configuración Prioridades

```csharp
// En IdentitySeeder.GetConfigurationValue():

// 1. Environment Variable (HIGHEST PRIORITY)
var envValue = Environment.GetEnvironmentVariable(key.Replace(":", "_").ToUpperInvariant());
// AdminUser:Password → ADMINUSER_PASSWORD

// 2. User Secrets (Recommended for Development)
var secretValue = configuration["AdminUser:Password"]; // from secrets.json

// 3. appsettings.*.json (LOWEST PRIORITY)
var appSettingsValue = configuration["AdminUser:Password"]; // from json files
```

---

## 🎯 Resumen Rápido

| Aspecto | Development | Staging | Production |
|---------|-------------|---------|------------|
| **Seed Automático** | ✅ Sí | ❌ No | ❌ No |
| **Password Source** | User Secrets | Env Variable | Env Variable |
| **Archivo Config** | appsettings.json + .Development.json | appsettings.Staging.json | appsettings.Production.json |
| **Connection String** | Local SQL Server | Staging Server | Production Server |
| **Logging Level** | Information | Warning | Error |
| **HTTPS** | In-process | Required | Required |

---

**Versión:** 1.0  
**Última actualización:** Febrero 2026  
**Autor:** Identity Configuration Guide
