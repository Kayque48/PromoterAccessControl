using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Services
{
    /// <summary>
    /// Serviço de BI (Business Intelligence) que agrega métricas de acesso e permanência.
    /// Fornece indicadores-chave de desempenho (KPIs) para análise de produtividade e alocação de recursos.
    /// Queries otimizadas com índices em DataHora, PromotorId, EmpresaId para performance em grandes volumes.
    /// </summary>
    public class DashboardService
    {
        private readonly PromotoresContext _context;

        public DashboardService(PromotoresContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retorna visão geral do dia (KPIs): promotores ativos, visitas, permanência média, acumulado 30d.
        /// Usa janela de 30 dias como período de análise para tendências sem sazonalidade.
        /// </summary>
        public async Task<DashboardResponse> GetDashboardHojeAsync()
        {
            var hoje = DateTime.UtcNow.Date;
            var dataLimite = hoje.AddDays(-30);

            // Total de promotores com registros de entrada hoje
            var totalPromotoresAtivos = await _context.Registros
                .Where(r => r.DataHora.Date == hoje && r.Tipo == "entrada")
                .Select(r => r.PromotorId)
                .Distinct()
                .CountAsync();

            // Total de visitas (entradas) hoje
            var totalVisitasHoje = await _context.Registros
                .Where(r => r.DataHora.Date == hoje && r.Tipo == "entrada")
                .CountAsync();

            // Média de horas por promotor (últimos 30 dias)
            var mediaHoras = await CalcularMediaHorasAsync(dataLimite, DateTime.UtcNow);

            // Total de registros últimos 30 dias (entradas)
            var totalRegistros = await _context.Registros
                .Where(r => r.DataHora >= dataLimite && r.Tipo == "entrada")
                .CountAsync();

            return new DashboardResponse
            {
                TotalPromotoresAtivos = totalPromotoresAtivos,
                TotalVisitasHoje = totalVisitasHoje,
                MediaHorasPorPromotor = (decimal)mediaHoras,
                TotalRegistrosUltimos30Dias = totalRegistros
            };
        }

        /// <summary>
        /// Analisa tendência de visitas na última semana (7 dias).
        /// Agrupa por data para identificar padrões de demanda (dias de pico vs baixa).
        /// </summary>
        public async Task<List<VisitasPorDiaResponse>> GetVisitasPorDiaSemanaAsync()
        {
            var ultimosDias = DateTime.UtcNow.Date.AddDays(-7);

            var visitasPorDia = await _context.Registros
                .Where(r => r.DataHora >= ultimosDias && r.Tipo == "entrada")
                .GroupBy(r => r.DataHora.Date)
                .Select(g => new VisitasPorDiaResponse
                {
                    Data = g.Key,
                    TotalVisitas = g.Count()
                })
                .OrderBy(v => v.Data)
                .ToListAsync();

            return visitasPorDia;
        }

        /// <summary>
        /// Ranking de promotores por permanência média (últimos 30 dias).
        /// PermanenciaMin calculado via diferença DataHora(saída) - DataHora(entrada).
        /// Identifica promotores com maior tempo de dedicação/produtividade por visita.
        /// </summary>
        public async Task<List<DuracaoMediaPorPromotorResponse>> GetDuracaoMediaPorPromotorAsync(int limitePromotores = 10)
        {
            var dataLimite = DateTime.UtcNow.AddDays(-30);

            var duracaoMedia = await _context.Registros
                .Include(r => r.Promotor)
                .Where(r => r.DataHora >= dataLimite && r.Tipo == "saida" && r.PermanenciaMin.HasValue)
                .GroupBy(r => new { r.PromotorId, r.Promotor.Nome })
                .Select(g => new DuracaoMediaPorPromotorResponse
                {
                    PromotorId = g.Key.PromotorId,
                    NomePromotor = g.Key.Nome,
                    DuracaoMediaMinutos = (decimal)g.Average(r => r.PermanenciaMin ?? 0)
                })
                .OrderByDescending(d => d.DuracaoMediaMinutos)
                .Take(limitePromotores)
                .ToListAsync();

            return duracaoMedia;
        }

        public async Task<List<RankingVisitasPorEmpresaResponse>> GetRankingVisitasPorEmpresaAsync()
        {
            var dataLimite = DateTime.UtcNow.Date.AddDays(-7);

            var ranking = await _context.Registros
                .Include(r => r.Empresa)
                .Where(r => r.DataHora >= dataLimite && r.Tipo == "entrada")
                .GroupBy(r => new { r.EmpresaId, r.Empresa.NomeFantasia })
                .Select(g => new RankingVisitasPorEmpresaResponse
                {
                    EmpresaId = g.Key.EmpresaId,
                    NomeEmpresa = g.Key.NomeFantasia,
                    TotalVisitas = g.Count()
                })
                .OrderByDescending(r => r.TotalVisitas)
                .ToListAsync();

            return ranking;
        }

        private async Task<double> CalcularMediaHorasAsync(DateTime dataInicio, DateTime dataFim)
        {
            var registros = await _context.Registros
                .Where(r => r.DataHora >= dataInicio && r.DataHora <= dataFim && r.Tipo == "saida" && r.PermanenciaMin.HasValue)
                .ToListAsync();

            if (registros.Count == 0) return 0;

            var totalMinutos = registros.Sum(r => r.PermanenciaMin ?? 0);
            return totalMinutos / 60.0 / Math.Max(registros.Select(r => r.PromotorId).Distinct().Count(), 1);
        }
    }
}
