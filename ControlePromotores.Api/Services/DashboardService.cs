using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Services
{
    public class DashboardService
    {
        private readonly PromotoresContext _context;

        public DashboardService(PromotoresContext context)
        {
            _context = context;
        }

        public async Task<DashboardResponse> GetDashboardHojeAsync()
        {
            var hoje = DateTime.UtcNow.Date;
            var dataLimite = hoje.AddDays(-30);

            // Total de promotores ativos (sem saída registrada hoje)
            var totalPromotoresAtivos = await _context.RegistrosAcesso
                .Where(r => r.Entrada >= hoje && r.Saida == null)
                .Select(r => r.PromotorId)
                .Distinct()
                .CountAsync();

            // Total de visitas hoje
            var totalVisitasHoje = await _context.RegistrosAcesso
                .Where(r => r.Entrada >= hoje)
                .CountAsync();

            // Média de horas por promotor (últimos 30 dias)
            var mediaHoras = await CalcularMediaHorasAsync(dataLimite, DateTime.UtcNow);

            // Total de registros últimos 30 dias
            var totalRegistros = await _context.RegistrosAcesso
                .Where(r => r.Entrada >= dataLimite)
                .CountAsync();

            return new DashboardResponse
            {
                TotalPromotoresAtivos = totalPromotoresAtivos,
                TotalVisitasHoje = totalVisitasHoje,
                MediaHorasPorPromotor = (decimal)mediaHoras,
                TotalRegistrosUltimos30Dias = totalRegistros
            };
        }

        public async Task<List<VisitasPorDiaResponse>> GetVisitasPorDiaSemanaAsync()
        {
            var ultimosDias = DateTime.UtcNow.Date.AddDays(-7);

            var visitasPorDia = await _context.RegistrosAcesso
                .Where(r => r.Entrada >= ultimosDias)
                .GroupBy(r => r.Entrada.Date)
                .Select(g => new VisitasPorDiaResponse
                {
                    Data = g.Key,
                    TotalVisitas = g.Count()
                })
                .OrderBy(v => v.Data)
                .ToListAsync();

            return visitasPorDia;
        }

        public async Task<List<DuracaoMediaPorPromotorResponse>> GetDuracaoMediaPorPromotorAsync(int limitePromotores = 10)
        {
            var dataLimite = DateTime.UtcNow.AddDays(-30);

            var duracaoMedia = await _context.RegistrosAcesso
                .Include(r => r.Promotor)
                .Where(r => r.Entrada >= dataLimite && r.Saida != null)
                .GroupBy(r => new { r.PromotorId, r.Promotor.Nome })
                .Select(g => new DuracaoMediaPorPromotorResponse
                {
                    PromotorId = g.Key.PromotorId,
                    NomePromotor = g.Key.Nome,
                    DuracaoMediaMinutos = (decimal)g.Average(r => r.TempoPermanenciaMinutos ?? 0)
                })
                .OrderByDescending(d => d.DuracaoMediaMinutos)
                .Take(limitePromotores)
                .ToListAsync();

            return duracaoMedia;
        }

        public async Task<List<RankingVisitasPorEmpresaResponse>> GetRankingVisitasPorEmpresaAsync()
        {
            var dataLimite = DateTime.UtcNow.Date.AddDays(-7);

            var ranking = await _context.RegistrosAcesso
                .Include(r => r.Promotor)
                .ThenInclude(p => p.Empresa)
                .Where(r => r.Entrada >= dataLimite)
                .GroupBy(r => new { r.Promotor.EmpresaId, r.Promotor.Empresa.NomeFantasia })
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
            var registros = await _context.RegistrosAcesso
                .Where(r => r.Entrada >= dataInicio && r.Entrada <= dataFim && r.Saida != null)
                .ToListAsync();

            if (registros.Count == 0) return 0;

            var totalMinutos = registros.Sum(r => r.TempoPermanenciaMinutos ?? 0);
            return totalMinutos / 60.0 / Math.Max(registros.Select(r => r.PromotorId).Distinct().Count(), 1);
        }
    }
}
