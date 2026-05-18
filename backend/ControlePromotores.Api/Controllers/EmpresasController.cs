using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControlePromotores.Api.Services;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresasController : ControllerBase
    {
        private readonly EmpresaService _empresaService;

        public EmpresasController(EmpresaService empresaService)
        {
            _empresaService = empresaService;
        }

        /// <summary>
        /// Obtém todas as empresas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<EmpresaResponse>>> GetAll()
        {
            try
            {
                var empresas = await _empresaService.GetAllAsync();
                return Ok(empresas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar empresas", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém uma empresa por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EmpresaResponse>> GetById(int id)
        {
            try
            {
                var empresa = await _empresaService.GetByIdAsync(id);
                if (empresa == null)
                    return NotFound(new { message = "Empresa não encontrada" });

                return Ok(empresa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar empresa", details = ex.Message });
            }
        }

        /// <summary>
        /// Cria uma nova empresa
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<EmpresaResponse>> Create([FromBody] CriarEmpresaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var empresa = await _empresaService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = empresa.Id }, empresa);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao criar empresa", details = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza uma empresa
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<EmpresaResponse>> Update(int id, [FromBody] AtualizarEmpresaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var empresa = await _empresaService.UpdateAsync(id, request);
                return Ok(empresa);
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
                return StatusCode(500, new { message = "Erro ao atualizar empresa", details = ex.Message });
            }
        }

        /// <summary>
        /// Deleta uma empresa (desativa)
        /// </summary>
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var resultado = await _empresaService.DeleteAsync(id);
                if (!resultado)
                    return NotFound(new { message = "Empresa não encontrada" });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao deletar empresa", details = ex.Message });
            }
        }
    }
}
