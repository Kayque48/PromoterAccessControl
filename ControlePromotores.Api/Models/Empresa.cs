using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
    public class Empresa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(14)]
        public string CNPJ { get; set; }

        [Required]
        [StringLength(150)]
        public string RazaoSocial { get; set; }

        [StringLength(150)]
        public string NomeFantasia { get; set; }

        [Required]
        [StringLength(20)]
        public string Telefone { get; set; }

        [Required]
        [StringLength(150)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Endereco { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

        // Relacionamentos
        public ICollection<Promotor> PromotoresExclusivos { get; set; } = new List<Promotor>();
        public ICollection<PromotorEmpresa> PromotorEmpresas { get; set; } = new List<PromotorEmpresa>();
        public ICollection<Registro> Registros { get; set; } = new List<Registro>();
        public ICollection<PromotorDocumento> PromotorDocumentos { get; set; } = new List<PromotorDocumento>();
    }
}