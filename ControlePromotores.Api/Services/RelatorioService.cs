using Microsoft.EntityFrameworkCore;
using System.Text;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Services
{
    /// <summary>
    /// Serviço de relatórios com filtragem por período, empresa e promotor.
    /// Suporta exportação em CSV para análise em ferramentas externas (Excel, Power BI).
    /// Agregações: total de visitas, permanência média, e contagem de promotores/empresas únicos.
    /// </summary>
    public class RelatorioService
    {
        private readonly PromotoresContext _context;

        public RelatorioService(PromotoresContext context)
        {
            _context = context;
        }

        public async Task<RelatorioAgregadoResponse> GetRelatorioAgregadoAsync(FiltroRelatorioRequest filtro)
        {
            // Query base: Filtra por período (DataInicio-DataFim) e tipo entrada (exclui saídas).
            // Relaciona Promotor e Empresa para dados de nome/empresa no relatório.
            var query = _context.Registros
                .Include(r => r.Promotor)
                .Include(r => r.Empresa)
                .Where(r => r.DataHora >= filtro.DataInicio && r.DataHora <= filtro.DataFim && r.Tipo == "entrada");

            // Filtros opcionais: Permitem drill-down por empresa específica ou promotor específico.
            // Caso base (null): Retorna relatório consolidado para todo o período/sistema.
            if (filtro.EmpresaId.HasValue)
            {
                query = query.Where(r => r.EmpresaId == filtro.EmpresaId.Value);
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
                EmpresaNome = r.Empresa.NomeFantasia ?? r.Empresa.RazaoSocial,
                Entrada = r.DataHora,
                Saida = null, // TODO: Implementar lógica para encontrar saída correspondente
                DuracaoMinutos = null
            }).ToList();

            var totalMinutos = registros.Where(r => r.PermanenciaMin.HasValue).Sum(r => r.PermanenciaMin) ?? 0;
            var countComDuracao = registros.Count(r => r.PermanenciaMin.HasValue);
            var mediaMinutos = countComDuracao > 0 ? (decimal)totalMinutos / countComDuracao : 0;

            return new RelatorioAgregadoResponse
            {
                TotalRegistros = registros.Count,
                DuracaoMediaMinutos = mediaMinutos,
                PromotoresUnicos = registros.Select(r => r.PromotorId).Distinct().Count(),
                EmpresasUnicos = registros.Select(r => r.EmpresaId).Distinct().Count(),
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
            if (relatorio.Registros != null)
            {
                foreach (var registro in relatorio.Registros)
                {
                    var saida = registro.Saida?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
                    sb.AppendLine($"{registro.Id},\"{registro.PromotorNome}\",\"{registro.EmpresaNome}\"," +
                        $"{registro.Entrada:yyyy-MM-dd HH:mm:ss},{saida},{registro.DuracaoMinutos ?? 0}");
                }
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
