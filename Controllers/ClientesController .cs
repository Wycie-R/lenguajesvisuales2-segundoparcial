using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiExamen.Data;
using WebApiExamen.DTOs;
using WebApiExamen.Models;

namespace WebApiExamen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(ApplicationDbContext context, ILogger<ClientesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/clientes
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ClienteResponseDto>> PostCliente([FromForm] ClienteDto clienteDto)
        {
            try
            {
                // Validar modelo
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar si el cliente ya existe
                if (await _context.Clientes.AnyAsync(c => c.CI == clienteDto.CI))
                {
                    return Conflict(new { message = "Ya existe un cliente con ese CI" });
                }

                // Crear el cliente
                var cliente = new Cliente
                {
                    CI = clienteDto.CI,
                    Nombres = clienteDto.Nombres,
                    Direccion = clienteDto.Direccion,
                    Telefono = clienteDto.Telefono,
                    FechaRegistro = DateTime.Now
                };

                // Procesar las fotografías
                if (clienteDto.FotoCasa1 != null)
                {
                    cliente.FotoCasa1 = await ConvertToBytes(clienteDto.FotoCasa1);
                }

                if (clienteDto.FotoCasa2 != null)
                {
                    cliente.FotoCasa2 = await ConvertToBytes(clienteDto.FotoCasa2);
                }

                if (clienteDto.FotoCasa3 != null)
                {
                    cliente.FotoCasa3 = await ConvertToBytes(clienteDto.FotoCasa3);
                }

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Cliente {cliente.CI} registrado exitosamente");

                var response = new ClienteResponseDto
                {
                    CI = cliente.CI,
                    Nombres = cliente.Nombres,
                    Direccion = cliente.Direccion,
                    Telefono = cliente.Telefono,
                    FechaRegistro = cliente.FechaRegistro,
                    TieneFotoCasa1 = cliente.FotoCasa1 != null,
                    TieneFotoCasa2 = cliente.FotoCasa2 != null,
                    TieneFotoCasa3 = cliente.FotoCasa3 != null,
                    CantidadArchivos = 0
                };

                return CreatedAtAction(nameof(GetCliente), new { ci = cliente.CI }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar cliente");
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteResponseDto>>> GetClientes()
        {
            try
            {
                var clientes = await _context.Clientes
                    .Include(c => c.Archivos)
                    .Select(c => new ClienteResponseDto
                    {
                        CI = c.CI,
                        Nombres = c.Nombres,
                        Direccion = c.Direccion,
                        Telefono = c.Telefono,
                        FechaRegistro = c.FechaRegistro,
                        TieneFotoCasa1 = c.FotoCasa1 != null,
                        TieneFotoCasa2 = c.FotoCasa2 != null,
                        TieneFotoCasa3 = c.FotoCasa3 != null,
                        CantidadArchivos = c.Archivos.Count
                    })
                    .ToListAsync();

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // GET: api/clientes/{ci}
        [HttpGet("{ci}")]
        public async Task<ActionResult<ClienteResponseDto>> GetCliente(string ci)
        {
            try
            {
                var cliente = await _context.Clientes
                    .Include(c => c.Archivos)
                    .FirstOrDefaultAsync(c => c.CI == ci);

                if (cliente == null)
                {
                    return NotFound(new { message = "Cliente no encontrado" });
                }

                var response = new ClienteResponseDto
                {
                    CI = cliente.CI,
                    Nombres = cliente.Nombres,
                    Direccion = cliente.Direccion,
                    Telefono = cliente.Telefono,
                    FechaRegistro = cliente.FechaRegistro,
                    TieneFotoCasa1 = cliente.FotoCasa1 != null,
                    TieneFotoCasa2 = cliente.FotoCasa2 != null,
                    TieneFotoCasa3 = cliente.FotoCasa3 != null,
                    CantidadArchivos = cliente.Archivos.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cliente {ci}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        // GET: api/clientes/{ci}/foto/{numeroFoto}
        [HttpGet("{ci}/foto/{numeroFoto}")]
        public async Task<IActionResult> GetFotoCliente(string ci, int numeroFoto)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(ci);

                if (cliente == null)
                {
                    return NotFound(new { message = "Cliente no encontrado" });
                }

                byte[]? fotoBytes = numeroFoto switch
                {
                    1 => cliente.FotoCasa1,
                    2 => cliente.FotoCasa2,
                    3 => cliente.FotoCasa3,
                    _ => null
                };

                if (fotoBytes == null)
                {
                    return NotFound(new { message = $"Foto {numeroFoto} no encontrada" });
                }

                return File(fotoBytes, "image/jpeg");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener foto {numeroFoto} del cliente {ci}");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        private async Task<byte[]> ConvertToBytes(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}