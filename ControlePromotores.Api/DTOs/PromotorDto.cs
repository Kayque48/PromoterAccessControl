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

        /// <summary>
        /// Tipo será ignorado nesta requisição. O tipo é determinado automaticamente:
        /// - Se EmpresaId > 0: promotor será 'exclusivo'
        /// - Se EmpresaId <= 0 ou null: promotor será 'promotor' (comum)
        /// </summary>
        [StringLength(20)]
        public string? Tipo { get; set; }

        /// <summary>
        /// ID da empresa exclusiva (se tipo exclusivo) ou da empresa inicial do vínculo (se comum).
        /// Opcional: pode ser null para criar promotor comum sem empresa inicial, vinculado depois.
        /// </summary>
        public int? EmpresaId { get; set; }

        /// <summary>
        /// Dias da semana permitidos em português: ["segunda", "terça", "quarta", "quinta", "sexta", "sábado", "domingo"].
        /// Será convertido para bitmask no servidor.
        /// </summary>
        public string[]? DiasPermitidos { get; set; }
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
        public int? EmpresaId { get; set; }
        public int[] EmpresaIds { get; set; } = Array.Empty<int>();
        public int? EmpresaExclusivaId { get; set; }
        public string[]? DiasPermitidos { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
        public bool Ativo { get; set; }
    }
}
