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
	public class PromotoresController: ControllerBase
	{
		private readonly PromotoresContext _context;

		public PromotoresController(PromotoresContext context)
		{
			_context = context;
        }
		[httpGet]
		public async Task<ActionResult> GetPromotores([FromQuery] int? empresaId)
		{
			var query = _context.Promotores.AsQueryable();
			.include (p => p.Empresa);
			.Asqueryable();

			if (empresaId.HasValue)
			{
				query = query.Where(p => p.EmpresaId == empresaId.Value);
				var promotores = await query.ToListAsync();
				return Ok(promotores);
            }

        }
		public async Task<ActionResult> CreatePromotor([FromBody] Promotor promotor)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
            }
        }
    }
}
