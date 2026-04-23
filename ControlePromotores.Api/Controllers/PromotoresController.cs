using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControlePromotores.Api.Services;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PromotoresController : ControllerBase
    {
        private readonly PromotorService _promotorService;

        public PromotoresController(PromotorService promotorService)
        {
            _promotorService = promotorService;
        }

        /// <summary>
        /// Obtém todos os promotores
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<PromotorResponse>>> GetAll([FromQuery] int? empresaId = null)
        {
            try
            {
                var promotores = await _promotorService.GetAllAsync(empresaId);
                return Ok(promotores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar promotores", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém um promotor por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PromotorResponse>> GetById(int id)
        {
            try
            {
                var promotor = await _promotorService.GetByIdAsync(id);
                if (promotor == null)
                    return NotFound(new { message = "Promotor não encontrado" });

                return Ok(promotor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar promotor", details = ex.Message });
            }
        }

        /// <summary>
        /// Cria um novo promotor
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<PromotorResponse>> Create([FromBody] CriarPromotorRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var promotor = await _promotorService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = promotor.Id }, promotor);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao criar promotor", details = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza um promotor
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<PromotorResponse>> Update(int id, [FromBody] AtualizarPromotorRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var promotor = await _promotorService.UpdateAsync(id, request);
                return Ok(promotor);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar promotor", details = ex.Message });
            }
        }

        /// <summary>
        /// Deleta um promotor (desativa)
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var resultado = await _promotorService.DeleteAsync(id);
                if (!resultado)
                    return NotFound(new { message = "Promotor não encontrado" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao deletar promotor", details = ex.Message });
            }
        }
    }
}
