using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [StringLength(50)]
        public string Login { get; set; }

        [Required]
        public string SenhaHash { get; set; }

        [Required]
        public string Perfil { get; set; } // "admin" ou "usuario"

        public bool Ativo { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime? UltimoLogin { get; set; }
    }
}
