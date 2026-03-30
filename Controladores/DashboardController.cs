using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.BD;

namespace ControlePromotores.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly PromotoresContext _context;

        public DashboardController(PromotoresContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDados([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim, [FromQuery] int? empresaId)
        {
            var query = _context.RegistrosAcesso.AsQueryable();
            if (dataInicio.HasValue)
                query = query.Where(r => r.Entrada >= dataInicio.Value.Date);
            if (dataFim.HasValue)
                query = query.Where(r => r.Entrada <= dataFim.Value.Date.AddDays(1).AddTicks(-1));

            var registros = await query.ToListAsync();

            var frequencia = registros
                .GroupBy(r => r.Entrada.DayOfWeek)
                .Select(g => new { dia = g.Key.ToString(), total = g.Count() })
                .ToList();

            var promotores = _context.Promotores.AsQueryable();
            if (empresaId.HasValue)
                promotores = promotores.Where(p => p.EmpresaId == empresaId.Value);

            var promotoresPorEmpresa = await promotores
                .GroupBy(p => p.Empresa.RazaoSocial)
                .Select(g => new { empresa = g.Key, quantidade = g.Count() })
                .ToListAsync();

            var tempoMedio = await _context.RegistrosAcesso
                .Include(r => r.Promotor)
                .ThenInclude(p => p.Empresa)
                .Where(r => r.Saida != null)
                .GroupBy(r => r.Promotor.Empresa.RazaoSocial)
                .Select(g => new { empresa = g.Key, media = g.Average(r => r.TempoPermanencia ?? 0) })
                .ToListAsync();

            var totalRegistros = await _context.RegistrosAcesso.CountAsync();
            var registrosHoje = await _context.RegistrosAcesso
                .Where(r => r.Entrada.Date == DateTime.Today)
                .CountAsync();
            var promotoresAtivos = await _context.Promotores
                .Where(p => p.Ativo)
                .CountAsync();
            var mediaHorasDia = await _context.RegistrosAcesso
                .Where(r => r.Saida != null && r.TempoPermanencia != null)
                .GroupBy(r => r.Entrada.Date)
                .Select(g => g.Average(r => r.TempoPermanencia ?? 0))
                .DefaultIfEmpty(0)
                .AverageAsync();

            return Ok(new {
                frequenciaSemana = frequencia,
                promotoresPorEmpresa = promotoresPorEmpresa,
                tempoMedioPorEmpresa = tempoMedio,
                totalRegistros,
                registrosHoje,
                promotoresAtivos,
                mediaHorasDia
            });
        }
    }
}
