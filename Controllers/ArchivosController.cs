using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using WebApiExamen.Data;
using WebApiExamen.Models;

namespace WebApiExamen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArchivosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ArchivosController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ArchivosController(ApplicationDbContext context, ILogger<ArchivosController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        // POST: api/archivos/upload/{ci}
        [HttpPost("upload/{ci}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UploadArchivos(string ci, IFormFile archivo)
        {
            try
            {
                // Verificar que el cliente existe
                var cliente = await _context.Clientes.FindAsync(ci);
                if (cliente == null)
                {
                    return NotFound(new { message = "Cliente no encontrado" });
                }

                // Validar que se envió un archivo
                if (archivo == null || archivo.Length == 0)
                {
                    return BadRequest(new { message = "No se recibió ningún archivo" });
                }

                // Validar que sea un archivo .zip
                if (!archivo.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "El archivo debe ser de tipo .zip" });
                }

                // Crear carpeta de uploads si no existe
                var uploadsPath = Path.Combine(_environment.ContentRootPath, "Uploads", ci);
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Guardar el archivo ZIP temporalmente
                var tempZipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
                using (var stream = new FileStream(tempZipPath, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                var archivosGuardados = new List<ArchivoCliente>();

                try
                {
                    // Extraer archivos del ZIP
                    using (var zipArchive = ZipFile.OpenRead(tempZipPath))
                    {
                        foreach (var entry in zipArchive.Entries)
                        {
                            // Ignorar carpetas vacías
                            if (string.IsNullOrEmpty(entry.Name))
                                continue;

                            // Generar nombre único para el archivo
                            var uniqueFileName = $"{Guid.NewGuid()}_{entry.Name}";
                            var filePath = Path.Combine(uploadsPath, uniqueFileName);

                            // Extraer el archivo
                            entry.ExtractToFile(filePath, overwrite: true);

                            // Crear registro en la base de datos
                            var archivoCliente = new ArchivoCliente
                            {
                                CICliente = ci,
                                NombreArchivo = entry.Name,
                                UrlArchivo = Path.Combine("Uploads", ci, uniqueFileName),
                                FechaCreacion = DateTime.Now
                            };

                            _context.ArchivosCliente.Add(archivoCliente);
                            archivosGuardados.Add(archivoCliente);
                        }
                    }

                    // Guardar los registros en la base de datos
                    await _context.SaveChangesAsync();

                    // Eliminar archivo temporal
                    System.IO.File.Delete(tempZipPath);

                    _logger.LogInformation($"Se cargaron {archivosGuardados.Count} archivos para el cliente {ci}");

                    return Ok(new
                    {
                        message = "Archivos cargados exitosamente",
                        totalArchivos = archivosGuardados.Count,
                        archivos = archivosGuardados.Select(a => new
                        {
                            a.IdArchivo,
                            a.NombreArchivo,
                            a.UrlArchivo,
                            a.FechaCreacion
                        })
                    });
                }
                catch (Exception ex)
                {
                    // Si hay error, eliminar archivo temporal si existe
                    if (System.IO.File.Exists(tempZipPath))
                    {
                        System.IO.File.Delete(tempZipPath);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar archivos para cliente {ci}");
                return StatusCode(500, new { message = "Error al procesar los archivos", error = ex.Message });
            }
        }

        // GET: api/archivos/cliente/{ci}
        [HttpGet("cliente/{ci}")]
        public async Task<ActionResult<IEnumerable<ArchivoCliente>>> GetArchivosPorCliente(string ci)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(ci);
                if (cliente == null)
                {
                    return NotFound(new { message = "Cliente no encontrado" });
                }

                var archivos = await _context.ArchivosCliente
                    .Where(a => a.CICliente == ci)
                    .OrderByDescending(a => a.FechaCreacion)
                    .Select(a => new
                    {
                        a.IdArchivo,
                        a.NombreArchivo,
                        a.UrlArchivo,
                        a.FechaCreacion
                    })
                    .ToListAsync();

                return Ok(new
                {
                    ci = ci,
                    totalArchivos = archivos.Count,
                    archivos = archivos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener archivos del cliente {ci}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // GET: api/archivos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult> GetArchivo(int id)
        {
            try
            {
                var archivo = await _context.ArchivosCliente.FindAsync(id);
                if (archivo == null)
                {
                    return NotFound(new { message = "Archivo no encontrado" });
                }

                var filePath = Path.Combine(_environment.ContentRootPath, archivo.UrlArchivo);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { message = "El archivo físico no existe" });
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                var contentType = GetContentType(archivo.NombreArchivo);
                return File(memory, contentType, archivo.NombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener archivo {id}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // DELETE: api/archivos/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteArchivo(int id)
        {
            try
            {
                var archivo = await _context.ArchivosCliente.FindAsync(id);
                if (archivo == null)
                {
                    return NotFound(new { message = "Archivo no encontrado" });
                }

                // Eliminar archivo físico
                var filePath = Path.Combine(_environment.ContentRootPath, archivo.UrlArchivo);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Eliminar registro de base de datos
                _context.ArchivosCliente.Remove(archivo);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Archivo {id} eliminado correctamente");

                return Ok(new { message = "Archivo eliminado correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar archivo {id}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".txt" => "text/plain",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".csv" => "text/csv",
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                _ => "application/octet-stream"
            };
        }
    }
}