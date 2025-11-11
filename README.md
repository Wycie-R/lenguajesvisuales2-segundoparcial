# Sistema de Gestión de Clientes y Archivos - Web API

## Descripción General
API REST desarrollada con ASP.NET Core Web API (.NET 8.0) que permite el registro y gestión de clientes, carga de archivos asociados y seguimiento de logs del sistema.

## Tecnologías Utilizadas
- **Framework**: ASP.NET Core Web API (.NET 8.0)
- **ORM**: Entity Framework Core 8.0 (Code First)
- **Base de Datos**: SQL Server
- **Documentación**: Swagger/OpenAPI
- **Manejo de Archivos**: IFormFile, System.IO.Compression

## Características Principales
1. ✅ Registro de clientes con fotografías
2. ✅ Carga múltiple de archivos (.zip)
3. ✅ Sistema de logging automático
4. ✅ Middleware de manejo de excepciones
5. ✅ Consulta de logs mediante API

## Estructura del Proyecto
```
WebApiExamen/
├── Controllers/
│   ├── ClientesController.cs
│   ├── ArchivosController.cs
│   └── LogsController.cs
├── Models/
│   ├── Cliente.cs
│   ├── ArchivoCliente.cs
│   └── LogApi.cs
├── Data/
│   └── ApplicationDbContext.cs
├── DTOs/
│   ├── ClienteDto.cs
│   └── LogApiDto.cs
├── Middleware/
│   └── LoggingMiddleware.cs
├── Services/
│   └── ArchivoService.cs
├── Uploads/
│   └── (archivos subidos)
├── appsettings.json
└── Program.cs
```

## Requisitos Previos
- .NET 8.0 SDK
- SQL Server (local o remoto)
- Visual Studio 2022 o VS Code

## Configuración Local

### 1. Clonar el repositorio
```bash
git clone https://github.com/[tu-usuario]/lenguajesvisuales2-segundoparcial.git
cd lenguajesvisuales2-segundoparcial
```

### 2. Configurar la cadena de conexión
Editar `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ClientesDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Restaurar paquetes NuGet
```bash
dotnet restore
```

### 4. Crear la base de datos
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Ejecutar la aplicación
```bash
dotnet run
```

La API estará disponible en: `https://localhost:7001` y `http://localhost:5001`

## Endpoints Disponibles

### 1. Clientes
- **POST** `/api/clientes` - Registrar nuevo cliente con fotografías
- **GET** `/api/clientes` - Listar todos los clientes
- **GET** `/api/clientes/{ci}` - Obtener cliente por CI

### 2. Archivos
- **POST** `/api/archivos/upload/{ci}` - Subir archivo ZIP con múltiples archivos
- **GET** `/api/archivos/cliente/{ci}` - Listar archivos de un cliente

### 3. Logs
- **GET** `/api/logs` - Consultar todos los logs
- **GET** `/api/logs/{id}` - Consultar log específico

## Ejemplos de Uso

### Registrar Cliente (Postman/Swagger)
```http
POST /api/clientes
Content-Type: multipart/form-data

CI: 12345678
Nombres: Juan Pérez
Direccion: Calle Principal 123
Telefono: 0981234567
FotoCasa1: [archivo imagen]
FotoCasa2: [archivo imagen]
FotoCasa3: [archivo imagen]
```

### Subir Archivos
```http
POST /api/archivos/upload/12345678
Content-Type: multipart/form-data

archivo: [archivo .zip]
```

## Modelos de Datos

### Cliente
- CI (string) - Primary Key
- Nombres (string)
- Direccion (string)
- Telefono (string)
- FotoCasa1 (byte[])
- FotoCasa2 (byte[])
- FotoCasa3 (byte[])

### ArchivoCliente
- IdArchivo (int) - Primary Key
- CICliente (string) - Foreign Key
- NombreArchivo (string)
- UrlArchivo (string)
- FechaCreacion (DateTime)

### LogApi
- IdLog (int) - Primary Key
- DateTime (DateTime)
- TipoLog (string)
- RequestBody (string)
- ResponseBody (string)
- UrlEndpoint (string)
- MetodoHttp (string)
- DireccionIp (string)
- Detalle (string)

## Despliegue en Hosting

### MonsterASP.NET (Hosting Gratuito)
1. Publicar el proyecto:
   ```bash
   dotnet publish -c Release
   ```

2. Crear base de datos en el hosting

3. Actualizar `appsettings.json` con la cadena de conexión remota

4. Subir archivos mediante FTP

5. Ejecutar migrations en el servidor

## Paquetes NuGet Requeridos
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```
## Autor
Giovanni Rojas

## Fecha de Entrega
10 / 11 / 2025

## Licencia
Universidad del Norte