using Microsoft.EntityFrameworkCore;
using WebApiExamen.Data;
using WebApiExamen.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllers();

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Gestión de Clientes y Archivos",
        Version = "v1",
        Description = "API REST para registro de clientes y gestión de archivos - Segundo Parcial",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Lenguajes Visuales II",
            Email = "estudiante@universidad.edu"
        }
    });
});

// Configurar CORS (si es necesario)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Configurar el pipeline HTTP

// Usar Swagger en todos los entornos (desarrollo y producción)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gestión de Clientes v1");
    c.RoutePrefix = string.Empty; // Swagger en la raíz
});

// Middleware de logging (ANTES de otros middlewares)
app.UseMiddleware<LoggingMiddleware>();

// Middleware de manejo de excepciones global
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var ex = error.Error;

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Ha ocurrido un error interno en el servidor",
                error = ex.Message,
                timestamp = DateTime.Now
            });
        }
    });
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Crear carpeta Uploads si no existe
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "Uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Mensaje de inicio
app.Logger.LogInformation("Aplicación iniciada correctamente");
app.Logger.LogInformation($"Swagger UI disponible en: {(app.Environment.IsDevelopment() ? "https://localhost:7001" : "/")}");

app.Run();