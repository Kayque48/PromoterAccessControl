using Microsoft.EntityFrameworkCore;
using System.Text;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Services
{
    public class RelatorioService
    {
        private readonly PromotoresContext _context;

        public RelatorioService(PromotoresContext context)
        {
            _context = context;
        }

        public async Task<RelatorioAgregadoResponse> GetRelatorioAgregadoAsync(FiltroRelatorioRequest filtro)
        {
            var query = _context.RegistrosAcesso
                .Include(r => r.Promotor)
                .ThenInclude(p => p.Empresa)
                .Where(r => r.Entrada >= filtro.DataInicio && r.Entrada <= filtro.DataFim);

            if (filtro.EmpresaId.HasValue)
            {
                query = query.Where(r => r.Promotor.EmpresaId == filtro.EmpresaId.Value);
            }

            if (filtro.PromotorId.HasValue)
            {
                query = query.Where(r => r.PromotorId == filtro.PromotorId.Value);
            }

            var registros = await query.ToListAsync();
            var registrosResponse = registros.Select(r => new RelatorioRegistroResponse
            {
                Id = r.Id,
                PromotorNome = r.Promotor.Nome,
                EmpresaNome = r.Promotor.Empresa.NomeFantasia,
                Entrada = r.Entrada,
                Saida = r.Saida,
                DuracaoMinutos = r.TempoPermanenciaMinutos
            }).ToList();

            var totalMinutos = registros.Where(r => r.TempoPermanenciaMinutos.HasValue).Sum(r => r.TempoPermanenciaMinutos);
            var countComDuracao = registros.Count(r => r.TempoPermanenciaMinutos.HasValue);
            var mediaMinutos = countComDuracao > 0 ? (decimal)totalMinutos / countComDuracao : 0;

            return new RelatorioAgregadoResponse
            {
                TotalRegistros = registros.Count,
                DuracaoMediaMinutos = mediaMinutos,
                PromotoresUnicos = registros.Select(r => r.PromotorId).Distinct().Count(),
                EmpresasUnicos = registros.Select(r => r.Promotor.EmpresaId).Distinct().Count(),
                Registros = registrosResponse
            };
        }

        public async Task<RelatorioCsvResponse> ExportarCSVAsync(FiltroRelatorioRequest filtro)
        {
            var relatorio = await GetRelatorioAgregadoAsync(filtro);
            var csv = GerarCSV(relatorio);
            var nomeArquivo = $"relatorio_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return new RelatorioCsvResponse
            {
                ConteudoArquivo = Encoding.UTF8.GetBytes(csv),
                NomeArquivo = nomeArquivo,
                ContentType = "text/csv"
            };
        }

        private string GerarCSV(RelatorioAgregadoResponse relatorio)
        {
            var sb = new StringBuilder();

            // Cabeçalho
            sb.AppendLine("Id,Promotor,Empresa,Entrada,Saída,Duração (minutos)");

            // Dados
            foreach (var registro in relatorio.Registros)
            {
                var saida = registro.Saida?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                sb.AppendLine($"{registro.Id},\"{registro.PromotorNome}\",\"{registro.EmpresaNome}\"," +
                    $"{registro.Entrada:yyyy-MM-dd HH:mm:ss},{saida},{registro.DuracaoMinutos ?? 0}");
            }

            // Resumo
            sb.AppendLine();
            sb.AppendLine("Resumo");
            sb.AppendLine($"Total de Registros,{relatorio.TotalRegistros}");
            sb.AppendLine($"Duração Média (minutos),{relatorio.DuracaoMediaMinutos:F2}");
            sb.AppendLine($"Promotores Únicos,{relatorio.PromotoresUnicos}");
            sb.AppendLine($"Empresas Únicas,{relatorio.EmpresasUnicos}");

            return sb.ToString();
        }
    }
}
