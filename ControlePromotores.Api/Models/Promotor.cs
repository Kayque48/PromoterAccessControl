using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
    public class Promotor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(11)]
        public string CPF { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [StringLength(20)]
        public string Telefone { get; set; }

        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)] // "promotor" ou "exclusivo"
        public string Tipo { get; set; } = "promotor";

        // Para promotor exclusivo
        public int? EmpresaExclusivaId { get; set; }
        public Empresa EmpresaExclusiva { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        public ICollection<PromotorEmpresa> PromotorEmpresas { get; set; } = new List<PromotorEmpresa>();
        public ICollection<Registro> Registros { get; set; } = new List<Registro>();
        public ICollection<PromotorDocumento> PromotorDocumentos { get; set; } = new List<PromotorDocumento>();
    }
}