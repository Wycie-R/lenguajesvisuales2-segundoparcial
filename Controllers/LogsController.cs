using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiExamen.Data;
using WebApiExamen.Models;

namespace WebApiExamen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LogsController> _logger;

        public LogsController(ApplicationDbContext context, ILogger<LogsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/logs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogApi>>> GetLogs(
            [FromQuery] string? tipoLog = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _context.LogsApi.AsQueryable();

                // Filtrar por tipo de log
                if (!string.IsNullOrEmpty(tipoLog))
                {
                    query = query.Where(l => l.TipoLog == tipoLog.ToUpper());
                }

                // Filtrar por rango de fechas
                if (fechaDesde.HasValue)
                {
                    query = query.Where(l => l.DateTime >= fechaDesde.Value);
                }

                if (fechaHasta.HasValue)
                {
                    query = query.Where(l => l.DateTime <= fechaHasta.Value);
                }

                // Obtener total de registros
                var totalRegistros = await query.CountAsync();

                // Aplicar paginación
                var logs = await query
                    .OrderByDescending(l => l.DateTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        l.IdLog,
                        l.DateTime,
                        l.TipoLog,
                        l.UrlEndpoint,
                        l.MetodoHttp,
                        l.DireccionIp,
                        l.Detalle,
                        // No incluir RequestBody y ResponseBody para reducir tamaño de respuesta
                        TieneRequestBody = !string.IsNullOrEmpty(l.RequestBody),
                        TieneResponseBody = !string.IsNullOrEmpty(l.ResponseBody)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    page = page,
                    pageSize = pageSize,
                    totalRegistros = totalRegistros,
                    totalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize),
                    logs = logs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener logs");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // GET: api/logs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<LogApi>> GetLog(int id)
        {
            try
            {
                var log = await _context.LogsApi.FindAsync(id);

                if (log == null)
                {
                    return NotFound(new { message = "Log no encontrado" });
                }

                return Ok(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener log {id}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // GET: api/logs/estadisticas
        [HttpGet("estadisticas")]
        public async Task<ActionResult> GetEstadisticas(
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var query = _context.LogsApi.AsQueryable();

                // Filtrar por rango de fechas
                if (fechaDesde.HasValue)
                {
                    query = query.Where(l => l.DateTime >= fechaDesde.Value);
                }

                if (fechaHasta.HasValue)
                {
                    query = query.Where(l => l.DateTime <= fechaHasta.Value);
                }

                var estadisticas = await query.GroupBy(l => l.TipoLog)
                    .Select(g => new
                    {
                        TipoLog = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToListAsync();

                var porMetodo = await query.GroupBy(l => l.MetodoHttp)
                    .Select(g => new
                    {
                        Metodo = g.Key,
                        Cantidad = g.Count()
                    })
                    .ToListAsync();

                var totalLogs = await query.CountAsync();

                return Ok(new
                {
                    totalLogs = totalLogs,
                    porTipo = estadisticas,
                    porMetodo = porMetodo,
                    fechaDesde = fechaDesde,
                    fechaHasta = fechaHasta
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // GET: api/logs/errores
        [HttpGet("errores")]
        public async Task<ActionResult> GetErrores([FromQuery] int top = 10)
        {
            try
            {
                var errores = await _context.LogsApi
                    .Where(l => l.TipoLog == "ERROR")
                    .OrderByDescending(l => l.DateTime)
                    .Take(top)
                    .Select(l => new
                    {
                        l.IdLog,
                        l.DateTime,
                        l.UrlEndpoint,
                        l.MetodoHttp,
                        l.DireccionIp,
                        l.Detalle
                    })
                    .ToListAsync();

                return Ok(new
                {
                    totalErrores = await _context.LogsApi.CountAsync(l => l.TipoLog == "ERROR"),
                    ultimosErrores = errores
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener errores");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // DELETE: api/logs/limpiar
        [HttpDelete("limpiar")]
        public async Task<ActionResult> LimpiarLogs([FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var query = _context.LogsApi.AsQueryable();

                if (fechaHasta.HasValue)
                {
                    query = query.Where(l => l.DateTime <= fechaHasta.Value);
                }
                else
                {
                    // Por defecto, eliminar logs de más de 30 días
                    var fecha30DiasAtras = DateTime.Now.AddDays(-30);
                    query = query.Where(l => l.DateTime <= fecha30DiasAtras);
                }

                var logsAEliminar = await query.ToListAsync();
                var cantidad = logsAEliminar.Count;

                _context.LogsApi.RemoveRange(logsAEliminar);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Se eliminaron {cantidad} logs");

                return Ok(new
                {
                    message = "Logs eliminados correctamente",
                    cantidadEliminada = cantidad
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar logs");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}