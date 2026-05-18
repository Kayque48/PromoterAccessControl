namespace ControlePromotores.Api.DTOs
{
    public class DashboardResponse
    {
        public int TotalPromotoresAtivos { get; set; }
        public int TotalVisitasHoje { get; set; }
        public decimal MediaHorasPorPromotor { get; set; }
        public int TotalRegistrosUltimos30Dias { get; set; }
    }

    public class VisitasPorDiaResponse
    {
        public DateTime Data { get; set; }
        public int TotalVisitas { get; set; }
    }

    public class DuracaoMediaPorPromotorResponse
    {
        public int PromotorId { get; set; }
        public required string NomePromotor { get; set; }
        public decimal DuracaoMediaMinutos { get; set; }
    }

    public class RankingVisitasPorEmpresaResponse
    {
        public int EmpresaId { get; set; }
        public required string NomeEmpresa { get; set; }
        public int TotalVisitas { get; set; }
    }
}
