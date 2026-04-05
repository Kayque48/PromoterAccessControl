namespace ControlePromotores.Api.DTOs
{
    public class RegistrarEntradaRequest
    {
        public int PromotorId { get; set; }
        public int EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        public string? Observacao { get; set; }
    }

    public class RegistrarSaidaRequest
    {
        public int PromotorId { get; set; }
        public int EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        public string? Observacao { get; set; }
    }

    public class RegistroResponse
    {
        public int Id { get; set; }
        public int PromotorId { get; set; }
        public string PromotorNome { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
        public string EmpresaNome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime DataHora { get; set; }
        public int? PermanenciaMin { get; set; }
        public int RegistradoPor { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string? Observacao { get; set; }
    }

    public class PromotorAtivoResponse
    {
        public int PromotorId { get; set; }
        public string PromotorNome { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
        public string EmpresaNome { get; set; } = string.Empty;
        public DateTime EntradaEm { get; set; }
        public int MinutosEmAtendimento { get; set; }
    }
}