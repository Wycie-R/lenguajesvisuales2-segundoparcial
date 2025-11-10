using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApiExamen.Models
{
    public class ArchivoCliente
    {
        [Key]
        public int IdArchivo { get; set; }

        [Required]
        [StringLength(20)]
        public string CICliente { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string NombreArchivo { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string UrlArchivo { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relación con Cliente
        [ForeignKey("CICliente")]
        public virtual Cliente? Cliente { get; set; }
    }
}