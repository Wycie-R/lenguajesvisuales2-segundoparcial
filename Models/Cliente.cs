using System.ComponentModel.DataAnnotations;

namespace WebApiExamen.Models
{
    public class Cliente
    {
        [Key]
        [Required]
        [StringLength(20)]
        public string CI { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;

        // Fotografías almacenadas como bytes (varbinary(max))
        public byte[]? FotoCasa1 { get; set; }

        public byte[]? FotoCasa2 { get; set; }

        public byte[]? FotoCasa3 { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Relación con archivos
        public virtual ICollection<ArchivoCliente> Archivos { get; set; } = new List<ArchivoCliente>();
    }
}