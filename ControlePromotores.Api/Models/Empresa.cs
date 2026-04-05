using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
    public class Empresa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(18)]
        public string CNPJ { get; set; }

        [Required]
        [StringLength(150)]
        public string RazaoSocial { get; set; }

        [Required]
        [StringLength(150)]
        public string NomeFantasia { get; set; }

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

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public bool Ativo { get; set; } = true;

        // Relacionamento com Promotores
        public ICollection<Promotor> Promotores { get; set; } = new List<Promotor>();
    }
}