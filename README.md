# Reto Técnico .NET - Microservicios

Solución completa de microservicios construida con .NET 8 que demuestra autenticación JWT, comunicación entre APIs, procesamiento de colas de mensajes y persistencia en base de datos con idempotencia.

## Descripción General de la Arquitectura

Esta solución implementa un sistema distribuido con los siguientes componentes:

- **AuthServer**: Servicio de autenticación JWT (mockeable)
- **ProductsApi**: API protegida con validación JWT y paginación
- **ProductsPublisherAPI**: Servicio publicador que orquesta el flujo completo
- **QueueWorker**: Servicio en segundo plano para procesamiento de mensajes con idempotencia
- **SQL Server**: Base de datos real para persistencia de datos

## Flujo del Sistema

1. **Autenticación**: PublisherAPI obtiene token JWT del AuthServer
2. **Obtención de Datos**: PublisherAPI consume productos del ProductsApi usando JWT
3. **Publicación de Mensajes**: Los productos se publican en la cola (un mensaje por producto)
4. **Procesamiento**: QueueWorker consume mensajes y persiste en base de datos
5. **Idempotencia**: El procesamiento repetido actualiza registros existentes en lugar de crear duplicados

## Tecnologías Utilizadas

- .NET 8 Web API (Controllers, no Minimal API)
- Entity Framework Core con SQL Server
- Autenticación JWT Bearer
- Containerización con Docker
- Background Services (Hosted Services)
- Patrón de Cola de Mensajes
- Logging estructurado con IDs de correlación

## Estructura del Repositorio

```
ProductsChallenge/
├── src/
│   ├── AuthServer/                    # Servicio de Autenticación JWT
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Dockerfile
│   │   └── Program.cs
│   ├── ProductsApi/                   # API de Productos Protegida
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Dockerfile
│   │   └── Program.cs
│   ├── ProductsPublisherAPI/          # Servicio Publicador
│   │   ├── Controllers/
│   │   ├── Models/
│   │   ├── Services/
│   │   ├── Data/
│   │   ├── Dockerfile
│   │   └── Program.cs
│   └── QueueWorker/                   # Procesador de Mensajes en Segundo Plano
│       ├── Models/
│       ├── Data/
│       ├── Worker.cs
│       ├── Dockerfile
│       └── Program.cs
├── docker/
│   └── docker-compose.yml            # Contenedor de SQL Server
└── README.md
```

## Prerrequisitos

- .NET 8 SDK
- Docker Desktop
- SQL Server (containerizado)
- Git

## Inicio Rápido

### 1. Clonar Repositorio

```bash
git clone <>
cd ProductsChallenge
```

### 2. Iniciar Infraestructura

```bash
# Iniciar SQL Server
cd docker
docker-compose up -d
```

### 3. Ejecutar Servicios

#### Opción A: Configuración Híbrida (Recomendada)

**Servicios en Docker (puertos fijos):**
```bash
# AuthServer - puerto 8081
cd src/AuthServer/AuthServer
docker build -t authserver .
docker run -d -p 8081:8080 --name authserver-container authserver

# ProductsApi - puerto 8082
cd src/ProductsApi/ProductsApi
docker build -t productsapi .
docker run -d -p 8082:8080 --name productsapi-container productsapi
```

**Servicios locales (puertos variables):**
```bash
# Terminal 1 - QueueWorker (puerto asignado automáticamente)
cd src/QueueWorker/QueueWorker
dotnet run

# Terminal 2 - ProductsPublisherAPI (puerto asignado automáticamente)
cd src/ProductsPublisherAPI/ProductsPublisherAPI
dotnet run
```

#### Opción B: Todo Local (todos los puertos son variables)

```bash
# Terminal 1 - AuthServer
cd src/AuthServer/AuthServer
dotnet run

# Terminal 2 - ProductsApi
cd src/ProductsApi/ProductsApi
dotnet run

# Terminal 3 - QueueWorker
cd src/QueueWorker/QueueWorker
dotnet run

# Terminal 4 - ProductsPublisherAPI
cd src/ProductsPublisherAPI/ProductsPublisherAPI
dotnet run
```

**IMPORTANTE:** En ejecución local, los puertos varían en cada inicio. Verifica en la consola para ver el puerto asignado:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5286
```

## Pruebas Manuales

### 1. Verificar AuthServer

**URL Docker:** `http://localhost:8081/swagger` (puerto fijo)
**URL Local:** Verificar puerto en consola, ejemplo: `http://localhost:5001/swagger`

**Prueba:**
```json
POST /auth/login
{
  "username": "testuser",
  "password": "testpass"
}
```

**Respuesta esperada:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
```

### 2. Verificar ProductsApi

**URL Docker:** `http://localhost:8082/swagger` (puerto fijo)
**URL Local:** Verificar puerto en consola del ProductsApi

**Pasos:**
1. Clic en "Authorize"
2. Ingresa: `Bearer [token-del-authserver]`
3. Ejecuta `GET /api/Products`

**Respuesta esperada:**
```json
{
  "items": [
    {
      "externalId": "p-1001",
      "name": "Mouse",
      "price": 15.99,
      "currency": "USD",
      "updatedAtUtc": "2025-09-13T..."
    }
  ],
  "page": 1,
  "pageSize": 50,
  "total": 5
}
```

### 3. Probar Flujo Completo

**URL:** Verificar puerto del PublisherAPI en la consola

**Prueba:**
```json
POST /api/publish/products
```

**Respuesta esperada:**
```json
{
  "message": "Successfully queued 5 products",
  "productCount": 5
}
```

**Verificar en logs del QueueWorker:**
```
Processing message msg-p-1001-... with correlation ...
Inserted new product p-1001 - CorrelationId: ...
```

### 4. Verificar Idempotencia

Ejecuta el mismo endpoint `POST /api/publish/products` nuevamente.

**En logs del QueueWorker debe mostrar:**
```
Updated existing product p-1001 - CorrelationId: ...
```

## Configuración de Puertos

### Docker (puertos fijos):
- **AuthServer**: 8081
- **ProductsApi**: 8082  
- **SQL Server**: 1433

### Local (puertos variables):
Los puertos se asignan automáticamente por .NET. Ejemplos:
- AuthServer: 5001, 5000, etc.
- ProductsApi: 5002, 5286, etc.
- PublisherAPI: 5003, 5432, etc.

## Variables de Configuración

**Para servicios en Docker:**
```json
{
  "AuthServer": { "BaseUrl": "http://host.docker.internal:8081" },
  "ProductsApi": { "BaseUrl": "http://host.docker.internal:8082" },
  "ConnectionStrings": { 
    "DefaultConnection": "Server=host.docker.internal,1433;Database=ProductsChallenge;User Id=sa;Password=MyStrongPass123!;TrustServerCertificate=true;" 
  }
}
```

**Para servicios locales:**
```json
{
  "AuthServer": { "BaseUrl": "http://localhost:8081" },
  "ProductsApi": { "BaseUrl": "http://localhost:8082" },
  "ConnectionStrings": { 
    "DefaultConnection": "Server=localhost,1433;Database=ProductsChallenge;Trusted_Connection=True;TrustServerCertificate=True;" 
  }
}
```

**QueueWorker:**
- Intervalo de consulta: 5 segundos
- Base de datos: ProductsChallenge
- Idempotencia: Basada en `externalId`

## Contratos de API

### AuthServer

**POST /auth/login**
```json
Solicitud: { "username": "string", "password": "string" }
Respuesta: { "access_token": "string", "expires_in": 3600 }
```

### ProductsApi

**GET /api/products**
```json
Respuesta: {
  "items": [
    {
      "externalId": "p-1001",
      "name": "Mouse",
      "price": 15.99,
      "currency": "USD",
      "updatedAtUtc": "2025-08-31T18:20:00Z"
    }
  ],
  "page": 1,
  "pageSize": 50,
  "total": 120
}
```

### PublisherAPI

**POST /api/publish/products**
```json
Respuesta: {
  "message": "Successfully queued X products",
  "productCount": 5
}
```

### Mensaje en Cola

```json
{
  "externalId": "p-1001",
  "name": "Mouse óptico",
  "price": 15.99,
  "currency": "USD",
  "fetchedAtUtc": "2025-08-31T18:25:30Z",
  "correlationId": "GUID",
  "source": "ProductsApi:/api/products"
}
```

## Solución de Problemas

### Error: Conexión a base de datos
**Causa:** SQL Server no está corriendo
**Solución:** 
```bash
docker ps
# Debe mostrar sqlserver-products corriendo
docker-compose up -d  # Si no está corriendo
```

### Error: Autenticación JWT
**Causa:** Token inválido o mal formato
**Solución:** Verificar que el token esté en formato `Bearer [token]`

### Error: PublisherAPI no encuentra AuthServer
**Causa:** URLs incorrectas en configuración
**Solución:** Verificar URLs en `appsettings.json`:
- Docker: `http://host.docker.internal:8081`
- Local: `http://localhost:[puerto-real]`

### Error: QueueWorker no procesa mensajes
**Causa:** Servicios usan bases de datos diferentes
**Solución:** Verificar que ambos usen la misma cadena de conexión

### Error: Puerto en uso
**Causa:** Servicio ya corriendo en ese puerto
**Solución:** 
```bash
docker stop [nombre-contenedor]
# O cambiar puerto en docker run -p 8082:8080
```

## Características Técnicas Implementadas

- Autenticación JWT entre servicios
- APIs RESTful con Controllers (no Minimal API)
- Validación de JWT en endpoints protegidos
- Paginación en API de productos
- Patrón Publisher-Subscriber con colas
- Procesamiento idempotente de mensajes
- Persistencia en base de datos real
- Logging estructurado con IDs de correlación
- Containerización parcial con Docker
- Background Services para procesamiento
- Entity Framework Core
- Manejo de errores y timeouts

## Comandos Útiles

### Docker
```bash
# Ver contenedores corriendo
docker ps

# Ver logs de un contenedor
docker logs [nombre-contenedor]

# Reiniciar contenedor
docker restart [nombre-contenedor]

# Parar todos los contenedores
docker stop authserver-container productsapi-container sqlserver-products
```

### Verificación rápida
```bash
# Verificar servicios Docker
curl http://localhost:8081/swagger
curl http://localhost:8082/swagger

# Verificar SQL Server
docker exec -it sqlserver-products /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P MyStrongPass123!
```

## Autor

Desarrollado por Yuly Andrea Morales.