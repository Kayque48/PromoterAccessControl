using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
    public class Registro
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
        [StringLength(20)] // "entrada" ou "saida"
        public string Tipo { get; set; }

        [Required]
        public DateTime DataHora { get; set; } = DateTime.UtcNow;

        public int? PermanenciaMin { get; set; }

        [Required]
        public int RegistradoPor { get; set; }
        public Usuario UsuarioRegistrador { get; set; }

        [StringLength(255)]
        public string Observacao { get; set; }
    }
}