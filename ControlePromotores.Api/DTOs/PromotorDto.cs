using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.DTOs
{
    public class CriarPromotorRequest
    {
        [Required]
        [StringLength(100)]
        public required string Nome { get; set; }

        [Required]
        [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "CPF deve estar no formato 000.000.000-00")]
        public required string CPF { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [Required]
        public int EmpresaId { get; set; }
    }

    public class AtualizarPromotorRequest : CriarPromotorRequest
    {
    }

    public class PromotorResponse
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string CPF { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public required string Tipo { get; set; }
        public int? EmpresaExclusivaId { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
        public bool Ativo { get; set; }
    }
}
