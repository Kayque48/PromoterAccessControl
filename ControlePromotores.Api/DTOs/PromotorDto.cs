using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.DTOs
{
    public class CriarPromotorRequest
    {
        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "CPF deve estar no formato 000.000.000-00")]
        public string CPF { get; set; }

        [StringLength(20)]
        public string Telefone { get; set; }

        [EmailAddress]
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

        [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "CEP deve estar no formato 00000-000")]
        public string CEP { get; set; }

        [Required]
        public int EmpresaId { get; set; }
    }

    public class AtualizarPromotorRequest : CriarPromotorRequest
    {
    }

    public class PromotorResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string CPF { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public string Endereco { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string CEP { get; set; }
        public DateTime DataContratacao { get; set; }
        public bool Ativo { get; set; }
        public int EmpresaId { get; set; }
    }
}
