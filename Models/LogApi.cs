using System.ComponentModel.DataAnnotations;

namespace WebApiExamen.Models
{
    public class LogApi
    {
        [Key]
        public int IdLog { get; set; }

        [Required]
        public DateTime DateTime { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string TipoLog { get; set; } = string.Empty; // "INFO", "ERROR", "WARNING"

        public string? RequestBody { get; set; }

        public string? ResponseBody { get; set; }

        [Required]
        [StringLength(500)]
        public string UrlEndpoint { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string MetodoHttp { get; set; } = string.Empty; // GET, POST, PUT, DELETE

        [StringLength(50)]
        public string? DireccionIp { get; set; }

        public string? Detalle { get; set; }
    }
}