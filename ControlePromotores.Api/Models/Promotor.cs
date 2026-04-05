using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
    public class Promotor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [StringLength(14)]
        public string CPF { get; set; }

        [StringLength(20)]
        public string Telefone { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [StringLength(300)]
        public string Endereco { get; set; }

        [StringLength(50)]
        public string Numero { get; set; }

        [StringLength(100)]
        public string Complemento { get; set; }

        [StringLength(50)]
        public string Bairro { get; set; }

        [StringLength(50)]
        public string Cidade { get; set; }

        [StringLength(2)]
        public string Estado { get; set; }

        [StringLength(10)]
        public string CEP { get; set; }

        public DateTime DataContratacao { get; set; } = DateTime.UtcNow;

        public bool Ativo { get; set; } = true;

        // Relacionamento com Empresa
        [Required]
        public int EmpresaId { get; set; }

        public Empresa Empresa { get; set; }

        // Relacionamento com Registros de Acesso
        public ICollection<RegistroAcesso> Registros { get; set; } = new List<RegistroAcesso>();
    }
}