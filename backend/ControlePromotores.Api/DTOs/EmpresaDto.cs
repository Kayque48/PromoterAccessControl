using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.DTOs
{
    public class CriarEmpresaRequest
    {
        [Required]
        [RegularExpression(@"^\d{2}\.\d{3}\.\d{3}/\d{4}-\d{2}$", ErrorMessage = "CNPJ deve estar no formato 00.000.000/0000-00")]
        public required string CNPJ { get; set; }

        [Required]
        [StringLength(150)]
        public required string RazaoSocial { get; set; }

        [StringLength(150)]
        public string? NomeFantasia { get; set; }

        [Required]
        [StringLength(20)]
        public required string Telefone { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public required string Email { get; set; }

        [Required]
        [StringLength(255)]
        public required string Endereco { get; set; }
    }

    public class AtualizarEmpresaRequest : CriarEmpresaRequest
    {
    }

    public class EmpresaResponse
    {
        public int Id { get; set; }
        public required string CNPJ { get; set; }
        public required string RazaoSocial { get; set; }
        public string? NomeFantasia { get; set; }
        public required string Telefone { get; set; }
        public required string Email { get; set; }
        public required string Endereco { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
        public bool Ativo { get; set; }
    }
}
