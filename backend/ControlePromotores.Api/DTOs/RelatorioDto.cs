namespace ControlePromotores.Api.DTOs
{
    public class FiltroRelatorioRequest
    {
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int? EmpresaId { get; set; }
        public int? PromotorId { get; set; }
    }

    public class RelatorioRegistroResponse
    {
        public int Id { get; set; }
        public required string PromotorNome { get; set; }
        public required string EmpresaNome { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime? Saida { get; set; }
        public int? DuracaoMinutos { get; set; }
    }

    public class RelatorioCsvResponse
    {
        public required byte[] ConteudoArquivo { get; set; }
        public required string NomeArquivo { get; set; }
        public required string ContentType { get; set; }
    }

    public class RelatorioAgregadoResponse
    {
        public int TotalRegistros { get; set; }
        public decimal DuracaoMediaMinutos { get; set; }
        public int PromotoresUnicos { get; set; }
        public int EmpresasUnicos { get; set; }
        public List<RelatorioRegistroResponse>? Registros { get; set; }
    }
}
