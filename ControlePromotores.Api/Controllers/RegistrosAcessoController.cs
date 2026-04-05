using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControlePromotores.Api.Services;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrosAcessoController : ControllerBase
    {
        private readonly RegistroAcessoService _registroService;

        public RegistrosAcessoController(RegistroAcessoService registroService)
        {
            _registroService = registroService;
        }

        /// <summary>
        /// Registra entrada de um promotor
        /// </summary>
        [HttpPost("entrada")]
        public async Task<ActionResult<RegistroAcessoResponse>> RegistrarEntrada([FromBody] RegistrarEntradaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var registro = await _registroService.RegistrarEntradaAsync(request.PromotorId);
                return CreatedAtAction(nameof(GetById), new { id = registro.Id }, registro);
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
                return StatusCode(500, new { message = "Erro ao registrar entrada", details = ex.Message });
            }
        }

        /// <summary>
        /// Registra saída de um promotor
        /// </summary>
        [HttpPost("saida")]
        public async Task<ActionResult<RegistroAcessoResponse>> RegistrarSaida([FromBody] RegistrarSaidaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var registro = await _registroService.RegistrarSaidaAsync(request.RegistroId);
                return Ok(registro);
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
                return StatusCode(500, new { message = "Erro ao registrar saída", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém lista de promotores ativos (sem saída)
        /// </summary>
        [HttpGet("ativos")]
        public async Task<ActionResult<List<PromotorAtativoResponse>>> GetPromotoresAtivos()
        {
            try
            {
                var promotores = await _registroService.GetPromotoresAtivosAsync();
                return Ok(promotores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar promotores ativos", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém registros de um promotor
        /// </summary>
        [HttpGet("promotor/{promotorId}")]
        public async Task<ActionResult<List<RegistroAcessoResponse>>> GetRegistrosByPromotor(
            int promotorId,
            [FromQuery] DateTime? dataInicio = null,
            [FromQuery] DateTime? dataFim = null)
        {
            try
            {
                var registros = await _registroService.GetRegistrosByPromotorAsync(promotorId, dataInicio, dataFim);
                return Ok(registros);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar registros", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém registros de uma empresa
        /// </summary>
        [HttpGet("empresa/{empresaId}")]
        public async Task<ActionResult<List<RegistroAcessoResponse>>> GetRegistrosByEmpresa(
            int empresaId,
            [FromQuery] DateTime? dataInicio = null,
            [FromQuery] DateTime? dataFim = null)
        {
            try
            {
                var registros = await _registroService.GetRegistrosByEmpresaAsync(empresaId, dataInicio, dataFim);
                return Ok(registros);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar registros", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém um registro por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RegistroAcessoResponse>> GetById(int id)
        {
            try
            {
                var registro = await _registroService.GetByIdAsync(id);
                if (registro == null)
                    return NotFound(new { message = "Registro não encontrado" });

                return Ok(registro);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar registro", details = ex.Message });
            }
        }
    }
}
