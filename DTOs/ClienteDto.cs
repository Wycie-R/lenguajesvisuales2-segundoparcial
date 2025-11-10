using System.ComponentModel.DataAnnotations;

namespace WebApiExamen.DTOs
{
    public class ClienteDto
    {
        [Required(ErrorMessage = "El CI es obligatorio")]
        [StringLength(20, ErrorMessage = "El CI no puede exceder 20 caracteres")]
        public string CI { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [StringLength(200, ErrorMessage = "Los nombres no pueden exceder 200 caracteres")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [StringLength(300, ErrorMessage = "La dirección no puede exceder 300 caracteres")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
        public string Telefono { get; set; } = string.Empty;

        public IFormFile? FotoCasa1 { get; set; }
        public IFormFile? FotoCasa2 { get; set; }
        public IFormFile? FotoCasa3 { get; set; }
    }

    public class ClienteResponseDto
    {
        public string CI { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public bool TieneFotoCasa1 { get; set; }
        public bool TieneFotoCasa2 { get; set; }
        public bool TieneFotoCasa3 { get; set; }
        public int CantidadArchivos { get; set; }
    }
}