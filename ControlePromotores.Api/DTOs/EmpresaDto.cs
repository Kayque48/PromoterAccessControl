using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.DTOs
{
    public class CriarEmpresaRequest
    {
        [Required]
        [RegularExpression(@"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$", ErrorMessage = "CNPJ deve estar no formato 00.000.000/0000-00")]
        public string CNPJ { get; set; }

        [Required]
        [StringLength(150)]
        public string RazaoSocial { get; set; }

        [Required]
        [StringLength(150)]
        public string NomeFantasia { get; set; }

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
    }

    public class AtualizarEmpresaRequest : CriarEmpresaRequest
    {
    }

    public class EmpresaResponse
    {
        public int Id { get; set; }
        public string CNPJ { get; set; }
        public string RazaoSocial { get; set; }
        public string NomeFantasia { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public string Endereco { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string CEP { get; set; }
        public DateTime DataCriacao { get; set; }
        public bool Ativo { get; set; }
    }
}
