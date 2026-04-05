using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
    public class PromotorDocumento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PromotorId { get; set; }
        public Promotor Promotor { get; set; }

        [Required]
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }

        [Required]
        [StringLength(80)]
        public string TipoDocumento { get; set; } // ex: "Contrato", "Carta de Apresentação"

        [Required]
        [StringLength(255)]
        public string Caminho { get; set; } // URL/Path do arquivo armazenado

        public DateTime EnviadoEm { get; set; } = DateTime.UtcNow;
    }
}
