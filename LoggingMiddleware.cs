using Microsoft.EntityFrameworkCore;
using System.Text;
using WebApiExamen.Data;
using WebApiExamen.Models;

namespace WebApiExamen.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            // Capturar información de la petición
            var request = context.Request;
            var originalBodyStream = context.Response.Body;

            string requestBody = string.Empty;
            string responseBody = string.Empty;

            try
            {
                // Leer el body de la petición (si existe)
                if (request.ContentLength > 0 && request.ContentType?.Contains("application/json") == true)
                {
                    request.EnableBuffering();
                    using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                    requestBody = await reader.ReadToEndAsync();
                    request.Body.Position = 0;
                }

                // Capturar la respuesta
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                // Ejecutar el siguiente middleware
                await _next(context);

                // Leer la respuesta
                memoryStream.Position = 0;
                responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);

                // Guardar log exitoso
                await SaveLog(dbContext, context, requestBody, responseBody, "INFO", null);
            }
            catch (Exception ex)
            {
                // Guardar log de error
                await SaveLog(dbContext, context, requestBody, null, "ERROR", ex.Message);

                _logger.LogError(ex, "Error en la ejecución del request");

                // Re-lanzar la excepción para que sea manejada por el middleware de excepciones
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task SaveLog(ApplicationDbContext dbContext, HttpContext context,
            string requestBody, string? responseBody, string tipoLog, string? detalle)
        {
            try
            {
                var log = new LogApi
                {
                    DateTime = DateTime.Now,
                    TipoLog = tipoLog,
                    RequestBody = TruncateString(requestBody, 4000),
                    ResponseBody = TruncateString(responseBody, 4000),
                    UrlEndpoint = $"{context.Request.Path}{context.Request.QueryString}",
                    MetodoHttp = context.Request.Method,
                    DireccionIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                    Detalle = detalle
                };

                dbContext.LogsApi.Add(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el log en la base de datos");
            }
        }

        private string? TruncateString(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}