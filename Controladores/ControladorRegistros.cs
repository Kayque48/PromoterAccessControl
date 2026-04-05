using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.Models;
using ControlePromotores.Api.BD;

namespace ControlePromotores.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrosController : ControllerBase
    {
        private readonly PromotoresContext _context;

        public RegistrosController(PromotoresContext context)
        {
            _context = context;
        }

        [HttpPost("entrada")]
        public async Task<IActionResult> RegistrarEntrada([FromBody] int promotorId)
        {
            var promotor = await _context.Promotores.FindAsync(promotorId);
            if (promotor == null)
                return NotFound(new { erro = "Promotor não encontrado" });

            var hoje = DateTime.Now.DayOfWeek;
            var diasPermitidos = promotor.DiasPermitidosList
                .Select(d => Enum.TryParse<DayOfWeek>(d, true, out var value) ? value : (DayOfWeek?)null)
                .Where(v => v.HasValue)
                .Select(v => v.Value)
                .ToList();

            if (diasPermitidos.Any() && !diasPermitidos.Contains(hoje))
            {
                return BadRequest(new { erro = "Promotor não está autorizado a registrar entrada no dia de hoje" });
            }

            var aberto = await _context.RegistrosAcesso
                .FirstOrDefaultAsync(r => r.PromotorId == promotorId && r.Saida == null);
            if (aberto != null)
                return BadRequest(new { erro = "Promotor já possui registro de entrada em aberto" });

            var registro = new RegistroAcesso
            {
                PromotorId = promotorId,
                Entrada = DateTime.Now
            };

            _context.RegistrosAcesso.Add(registro);
            await _context.SaveChangesAsync();
            return Ok(registro);
        }

        [HttpPut("saida/{id}")]
        public async Task<IActionResult> RegistrarSaida(int id)
        {
            var registro = await _context.RegistrosAcesso.FindAsync(id);
            if (registro == null)
                return NotFound(new { erro = "Registro não encontrado" });

            if (registro.Saida != null)
                return BadRequest(new { erro = "Saída já registrada" });

            registro.Saida = DateTime.Now;
            registro.TempoPermanencia = (int)(registro.Saida.Value - registro.Entrada).TotalMinutes;

            await _context.SaveChangesAsync();
            return Ok(registro);
        }

        [HttpGet("promotor/{promotorId}")]
        public async Task<IActionResult> GetRegistrosPorPromotor(int promotorId)
        {
            var registros = await _context.RegistrosAcesso
                .Where(r => r.PromotorId == promotorId)
                .OrderByDescending(r => r.Entrada)
                .ToListAsync();
            return Ok(registros);
        }

        [HttpGet("abertos")]
        public async Task<IActionResult> GetRegistrosAbertos()
        {
            var abertos = await _context.RegistrosAcesso
                .Include(r => r.Promotor)
                .ThenInclude(p => p.Empresa)
                .Where(r => r.Saida == null)
                .OrderBy(r => r.Entrada)
                .Select(r => new
                {
                    r.Id,
                    Promotor = r.Promotor.Nome,
                    Empresa = r.Promotor.Empresa.RazaoSocial,
                    Entrada = r.Entrada
                })
                .ToListAsync();

            return Ok(abertos);
        }

        [HttpGet]
        public async Task<IActionResult> GetRegistrosHistorico([FromQuery] DateTime? dataInicio, [FromQuery] DateTime? dataFim, [FromQuery] int? empresaId, [FromQuery] int? promotorId, [FromQuery] string? busca)
        {
            var query = _context.RegistrosAcesso
                .Include(r => r.Promotor)
                .ThenInclude(p => p.Empresa)
                .AsQueryable();

            if (dataInicio.HasValue)
                query = query.Where(r => r.Entrada >= dataInicio.Value.Date);

            if (dataFim.HasValue)
                query = query.Where(r => r.Entrada <= dataFim.Value.Date.AddDays(1).AddTicks(-1));

            if (empresaId.HasValue)
                query = query.Where(r => r.Promotor.EmpresaId == empresaId.Value);

            if (promotorId.HasValue)
                query = query.Where(r => r.PromotorId == promotorId.Value);

            if (!string.IsNullOrWhiteSpace(busca))
                query = query.Where(r => r.Promotor.Nome.Contains(busca) || r.Promotor.Empresa.RazaoSocial.Contains(busca));

            var registros = await query
                .OrderByDescending(r => r.Entrada)
                .ToListAsync();

            var resultado = registros.Select(r => new
            {
                Promotor = r.Promotor.Nome,
                Empresa = r.Promotor.Empresa.RazaoSocial,
                Data = r.Entrada.ToString("dd/MM/yyyy"),
                DiaSemana = r.Entrada.ToString("dddd"),
                Entrada = r.Entrada.ToString("HH:mm"),
                Saida = r.Saida?.ToString("HH:mm"),
                Duracao = r.Saida.HasValue
                    ? $"{(r.TempoPermanencia ?? 0) / 60}h {(r.TempoPermanencia ?? 0) % 60}min"
                    : "-",
                DiaCorreto = r.Promotor.DiasPermitidosList
                    .Select(d => Enum.Parse<DayOfWeek>(d, true))
                    .Contains(r.Entrada.DayOfWeek),
                DiasPermitidos = r.Promotor.DiasPermitidosList
            }).ToList();

            return Ok(resultado);
        }
    }
}