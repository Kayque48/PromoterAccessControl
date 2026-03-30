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
    public class PromotoresController : ControllerBase
    {
        private readonly PromotoresContext _context;

        public PromotoresController(PromotoresContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Promotor>>> GetPromotores([FromQuery] int? empresaId)
        {
            var query = _context.Promotores.Include(p => p.Empresa).AsQueryable();
            if (empresaId.HasValue)
                query = query.Where(p => p.EmpresaId == empresaId.Value);
            return Ok(await query.ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult<Promotor>> CreatePromotor([FromBody] Promotor promotor)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (promotor.Categoria?.Equals("Promotor Exclusivo", StringComparison.OrdinalIgnoreCase) == true)
            {
                var exclusivos = await _context.Promotores.CountAsync(p => p.Categoria == "Promotor Exclusivo" && p.Ativo);
                if (exclusivos >= 10)
                {
                    return BadRequest(new { erro = "Limite de 10 promotores exclusivos atingido." });
                }
            }

            promotor.Ativo = true;
            _context.Promotores.Add(promotor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPromotor), new { id = promotor.Id }, promotor);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Promotor>> GetPromotor(int id)
        {
            var promotor = await _context.Promotores.Include(p => p.Empresa).FirstOrDefaultAsync(p => p.Id == id);
            if (promotor == null) return NotFound();
            return Ok(promotor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePromotor(int id, [FromBody] Promotor promotor)
        {
            if (id != promotor.Id)
                return BadRequest(new { erro = "ID incompatível" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existente = await _context.Promotores.FindAsync(id);
            if (existente == null)
                return NotFound(new { erro = "Promotor não encontrado" });

            if (promotor.Categoria?.Equals("Promotor Exclusivo", StringComparison.OrdinalIgnoreCase) == true)
            {
                var exclusivos = await _context.Promotores.CountAsync(p => p.Categoria == "Promotor Exclusivo" && p.Ativo && p.Id != id);
                if (exclusivos >= 10)
                {
                    return BadRequest(new { erro = "Limite de 10 promotores exclusivos atingido." });
                }
            }

            existente.Nome = promotor.Nome;
            existente.Telefone = promotor.Telefone;
            existente.Email = promotor.Email;
            existente.EmpresaId = promotor.EmpresaId;
            existente.Ativo = promotor.Ativo;
            existente.Cpf = promotor.Cpf;
            existente.Categoria = promotor.Categoria;
            existente.Endereco = promotor.Endereco;
            existente.DiasPermitidos = promotor.DiasPermitidos;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromotor(int id)
        {
            var promotor = await _context.Promotores.FindAsync(id);
            if (promotor == null)
                return NotFound(new { erro = "Promotor não encontrado" });

            // Soft delete
            promotor.Ativo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("empresas")]
        public async Task<ActionResult<IEnumerable<Empresa>>> GetEmpresas()
        {
            return Ok(await _context.Empresas.ToListAsync());
        }

        [HttpPost("empresas")]
        public async Task<ActionResult<Empresa>> CreateEmpresa([FromBody] Empresa empresa)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEmpresas), new { id = empresa.Id }, empresa);
        }

        [HttpPut("empresas/{id}")]
        public async Task<IActionResult> UpdateEmpresa(int id, [FromBody] Empresa empresa)
        {
            if (id != empresa.Id)
                return BadRequest(new { erro = "ID incompatível" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existente = await _context.Empresas.FindAsync(id);
            if (existente == null)
                return NotFound(new { erro = "Empresa não encontrada" });

            existente.RazaoSocial = empresa.RazaoSocial;
            existente.NomeFantasia = empresa.NomeFantasia;
            existente.Cnpj = empresa.Cnpj;
            existente.Telefone = empresa.Telefone;
            existente.EmailCorporativo = empresa.EmailCorporativo;
            existente.Endereco = empresa.Endereco;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("empresas/{id}")]
        public async Task<IActionResult> DeleteEmpresa(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null)
                return NotFound(new { erro = "Empresa não encontrada" });

            _context.Empresas.Remove(empresa);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
