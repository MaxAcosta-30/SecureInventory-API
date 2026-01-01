# SecureInventory API

API REST de gestión de inventario segura construida con .NET 10, Dapper (SQL Server), Redis (patrón Cache-Aside) y autenticación JWT.

## 📋 Descripción

SecureInventory es una API robusta diseñada para gestionar productos de inventario con un enfoque en seguridad, rendimiento y escalabilidad. Implementa autenticación basada en tokens JWT, almacenamiento de datos en SQL Server mediante Dapper, y un sistema de caché distribuido con Redis siguiendo el patrón Cache-Aside para optimizar el rendimiento de las consultas.

### Características Principales

- 🔐 **Autenticación JWT**: Sistema de autenticación seguro basado en tokens JWT
- 💾 **SQL Server con Dapper**: Acceso a datos eficiente y seguro mediante micro-ORM Dapper
- ⚡ **Redis Cache-Aside**: Sistema de caché distribuido para optimizar lecturas frecuentes
- 🏗️ **Arquitectura en Capas**: Separación clara de responsabilidades (Core/Infrastructure/API)
- 🐳 **Docker Compose**: Infraestructura containerizada lista para desarrollo

## 🏗️ Arquitectura de Capas

El proyecto sigue una arquitectura en capas que separa las responsabilidades:

```
SecureInventory/
├── SecureInventory.Core/          # Capa de Dominio
│   ├── Entities/                  # Entidades del dominio (User, Product)
│   └── Interfaces/                # Contratos de repositorios (IUserRepository, IProductRepository)
│
├── SecureInventory.Infrastructure/ # Capa de Infraestructura
│   └── Repositories/              # Implementaciones de repositorios (Dapper + Redis)
│
└── SecureInventory.Api/           # Capa de Presentación
    ├── Controllers/               # Controladores REST (AuthController, ProductsController)
    └── Program.cs                 # Configuración de servicios y middleware
```

### Descripción de Capas

#### **SecureInventory.Core**
- **Responsabilidad**: Contiene la lógica de dominio y contratos
- **Contenido**: 
  - Entidades (`User`, `Product`)
  - Interfaces de repositorios (`IUserRepository`, `IProductRepository`)
- **Dependencias**: Ninguna (capa independiente)

#### **SecureInventory.Infrastructure**
- **Responsabilidad**: Implementación de acceso a datos y servicios externos
- **Contenido**:
  - `UserRepository`: Implementación con Dapper para SQL Server
  - `ProductRepository`: Implementación con Dapper + Redis (Cache-Aside)
- **Dependencias**: `SecureInventory.Core`, Dapper, Microsoft.Data.SqlClient, StackExchange.Redis

#### **SecureInventory.Api**
- **Responsabilidad**: Exponer endpoints REST y coordinar la lógica de negocio
- **Contenido**:
  - `AuthController`: Registro y autenticación de usuarios
  - `ProductsController`: CRUD de productos (con caché)
  - `Program.cs`: Configuración de servicios, JWT, middleware
- **Dependencias**: `SecureInventory.Core`, `SecureInventory.Infrastructure`, ASP.NET Core, JWT Bearer

## 🚀 Requisitos de Infraestructura

### Prerequisitos

- **.NET 10 SDK** o superior
- **Docker Desktop** (para SQL Server y Redis)
- **Git** (opcional)

### Servicios Docker

La aplicación utiliza Docker Compose para orquestar los siguientes servicios:

1. **SQL Server 2022**: Base de datos relacional
   - Puerto: `1440` (configurable vía variable de entorno `SQL_PORT`)
   - Contraseña SA: Configurable vía variable de entorno `MSSQL_SA_PASSWORD`
   - Base de datos: `SecureInventoryDB` (creada automáticamente)

2. **Redis Alpine**: Caché en memoria
   - Puerto: `6379` (configurable vía variable de entorno `REDIS_PORT`)

## 📦 Guía de Inicio Rápido

### 1. Configuración del Entorno

Crea un archivo `.env` en la raíz del proyecto (opcional, puedes usar variables de entorno directamente):

```bash
MSSQL_SA_PASSWORD=TuPasswordSeguro123!
SQL_PORT=1440
REDIS_PORT=6379
DB_SERVER=localhost
```

### 2. Iniciar Infraestructura con Docker

```bash
# Navegar a la raíz del proyecto
cd SecureInventory

# Iniciar contenedores (SQL Server y Redis)
docker-compose up -d

# Verificar que los contenedores están corriendo
docker ps
```

### 3. Inicializar Base de Datos

Ejecuta el script de inicialización `init.sql` para crear las tablas y datos semilla:

```bash
# Conectarse al contenedor de SQL Server
docker exec -it secure_inventory_db /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "TuPasswordSeguro123!" -i /var/opt/mssql/data/init.sql
```

**Alternativa**: Si tienes SQL Server Management Studio o Azure Data Studio, conecta a `localhost,1440` con usuario `sa` y ejecuta manualmente el contenido de `init.sql`.

### 4. Configurar Variables de Entorno (Opcional)

Si no usas `.env`, configura las variables de entorno en tu sistema:

```bash
# Windows (PowerShell)
$env:MSSQL_SA_PASSWORD="TuPasswordSeguro123!"
$env:SQL_PORT="1440"
$env:REDIS_PORT="6379"

# Linux/Mac
export MSSQL_SA_PASSWORD="TuPasswordSeguro123!"
export SQL_PORT="1440"
export REDIS_PORT="6379"
```

### 5. Restaurar Dependencias y Ejecutar API

```bash
# Navegar a la carpeta de la API
cd src/SecureInventory.Api

# Restaurar paquetes NuGet
dotnet restore

# Ejecutar la aplicación
dotnet run
```

La API estará disponible en: `https://localhost:5001` o `http://localhost:5000` (dependiendo de tu configuración).

### 6. Verificar Endpoints

Una vez iniciada la API, puedes acceder a la documentación OpenAPI en:
- Swagger UI: `https://localhost:5001/swagger` (si está habilitado)
- OpenAPI JSON: `https://localhost:5001/openapi/v1.json`

## 🔐 Estrategia de Caché con Redis (Patrón Cache-Aside)

El sistema implementa el patrón **Cache-Aside** (también conocido como Lazy Loading) para optimizar las consultas de productos:

### Flujo Cache-Aside

```
Cliente → API → ProductRepository.GetByIdAsync(id)
                     ↓
             1. Consultar Redis (clave: "product:{id}")
                     ↓
         ┌───────────┴───────────┐
         │                       │
    CACHE HIT              CACHE MISS
         │                       │
    Deserializar          Consultar SQL Server
         │                       │
    Retornar Producto     Serializar a JSON
                               │
                         Almacenar en Redis (TTL: 10 min)
                               │
                         Retornar Producto
```

### Características

- **TTL (Time To Live)**: Los productos en caché expiran después de 10 minutos
- **Serialización**: Los productos se almacenan como JSON en Redis
- **Clave de Caché**: Formato `product:{id}` (ejemplo: `product:1`)
- **Fallback Automático**: Si Redis no está disponible, se consulta directamente SQL Server

### Ventajas del Patrón Cache-Aside

✅ **Rendimiento**: Reduce significativamente la carga en SQL Server para consultas frecuentes  
✅ **Resiliencia**: Si Redis falla, la aplicación sigue funcionando  
✅ **Simplicidad**: Lógica de caché explícita y fácil de entender  
✅ **Flexibilidad**: Permite invalidación manual del caché si es necesario

### Consideraciones

⚠️ **Consistencia**: Cuando se crea/actualiza un producto, el caché no se invalida automáticamente. Los cambios se reflejarán después del TTL o se requiere invalidación manual.

## 📡 Endpoints y Ejemplos con cURL

### 1. Registro de Usuario

Registra un nuevo usuario en el sistema.

```bash
curl -X POST "https://localhost:5001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "usuario_test",
    "password": "Password123!"
  }'
```

**Respuesta Exitosa (200 OK)**:
```json
"Usuario registrado exitosamente."
```

**Respuesta de Error (400 BadRequest)**:
```json
"El usuario ya existe."
```

### 2. Inicio de Sesión (Login)

Autentica un usuario y obtén un token JWT.

```bash
curl -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "usuario_test",
    "password": "Password123!"
  }'
```

**Respuesta Exitosa (200 OK)**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Respuesta de Error (401 Unauthorized)**:
```json
"Credenciales inválidas."
```

### 3. Obtener Producto por ID (Con Caché)

Obtiene un producto del inventario. La primera consulta consultará SQL Server, las siguientes (dentro de 10 minutos) vendrán de Redis.

```bash
# Primera consulta (CACHE MISS - consulta SQL Server)
curl -X GET "https://localhost:5001/api/products/1" \
  -H "Accept: application/json"

# Segunda consulta (CACHE HIT - consulta Redis)
curl -X GET "https://localhost:5001/api/products/1" \
  -H "Accept: application/json"
```

**Respuesta Exitosa (200 OK)**:
```json
{
  "id": 1,
  "name": "Laptop Gamer Linux",
  "price": 1500.00,
  "stock": 10
}
```

**Respuesta de Error (404 NotFound)**:
```json
"Producto no encontrado."
```

### 4. Crear Producto (Requiere Autenticación)

Crea un nuevo producto en el inventario. Requiere un token JWT válido.

```bash
# Guardar el token en una variable (después del login)
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Crear producto
curl -X POST "https://localhost:5001/api/products" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Teclado Mecánico RGB",
    "price": 89.99,
    "stock": 25
  }'
```

**Respuesta Exitosa (201 Created)**:
```json
{
  "id": 2,
  "name": "Teclado Mecánico RGB",
  "price": 89.99,
  "stock": 25
}
```

**Respuesta de Error (401 Unauthorized)**:
```json
"Unauthorized"
```

**Respuesta de Error (400 BadRequest)**:
```json
{
  "errors": {
    "Price": ["El precio debe ser mayor o igual a cero."]
  }
}
```

### Flujo Completo: Login → Crear → Consultar con Caché

```bash
# 1. Login y guardar token
LOGIN_RESPONSE=$(curl -s -X POST "https://localhost:5001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "usuario_test", "password": "Password123!"}')

TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*' | cut -d'"' -f4)

# 2. Crear producto usando el token
curl -X POST "https://localhost:5001/api/products" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "name": "Monitor 4K",
    "price": 299.99,
    "stock": 15
  }'

# 3. Consultar producto (primera vez - CACHE MISS)
curl -X GET "https://localhost:5001/api/products/1" \
  -H "Accept: application/json"

# 4. Consultar producto nuevamente (CACHE HIT desde Redis)
curl -X GET "https://localhost:5001/api/products/1" \
  -H "Accept: application/json"
```

## 🛠️ Tecnologías Utilizadas

- **.NET 10**: Framework de desarrollo
- **ASP.NET Core**: Framework web
- **Dapper 2.1.66**: Micro-ORM para SQL Server
- **StackExchange.Redis 2.10.1**: Cliente Redis
- **BCrypt.Net-Next 4.0.3**: Hashing de contraseñas
- **Microsoft.AspNetCore.Authentication.JwtBearer 10.0.1**: Autenticación JWT
- **SQL Server 2022**: Base de datos relacional
- **Redis Alpine**: Caché en memoria
- **Docker Compose**: Orquestación de contenedores

## 📝 Estructura de Base de Datos

### Tabla: Users

| Columna       | Tipo          | Descripción                          |
|---------------|---------------|--------------------------------------|
| Id            | INT IDENTITY  | Identificador único (clave primaria) |
| Username      | NVARCHAR(50)  | Nombre de usuario único              |
| PasswordHash  | NVARCHAR(255) | Hash de contraseña (BCrypt)          |
| Role          | NVARCHAR(20)  | Rol del usuario (Admin, User)        |

### Tabla: Products

| Columna | Tipo          | Descripción                          |
|---------|---------------|--------------------------------------|
| Id      | INT IDENTITY  | Identificador único (clave primaria) |
| Name    | NVARCHAR(100) | Nombre del producto                  |
| Price   | DECIMAL(18,2) | Precio unitario (≥ 0)                |
| Stock   | INT           | Cantidad en inventario (≥ 0)         |

## 🔒 Seguridad

- **Hashing de Contraseñas**: BCrypt con salt automático
- **JWT**: Tokens firmados con HMAC-SHA256, expiración de 1 hora
- **SQL Injection Prevention**: Parámetros tipados de Dapper
- **HTTPS**: Redirección automática a HTTPS en producción
- **Validación de Datos**: Validación a nivel de base de datos y aplicación

## 📄 Licencia

Este proyecto es de código abierto y está disponible bajo la licencia MIT.

## 👨‍💻 Autor

Desarrollado como proyecto de ejemplo de arquitectura .NET con caché distribuido.

---

**Nota**: Para producción, asegúrate de:
- Cambiar la clave JWT por una segura almacenada en variables de entorno o Azure Key Vault
- Configurar HTTPS con certificados válidos
- Implementar logging estructurado
- Agregar monitoreo y alertas
- Configurar backup automático de la base de datos

