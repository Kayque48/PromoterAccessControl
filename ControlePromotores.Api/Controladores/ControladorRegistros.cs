using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.Data;
using ControlePromotores.Api.Models;

namespace ControlePromotores.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RegistrosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("entrada")]
        public async Task<IActionResult> RegistrarEntrada([FromBody] int promotorId)
        {
            var promotor = await _context.Promotores.FindAsync(promotorId);
            if (promotor == null)
                return NotFound(new { erro = "Promotor não encontrado" });

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
    }
}