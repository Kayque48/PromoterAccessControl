using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        /// Obtém históricos de registros (entradas e saídas agrupadas por sessão)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<RegistroSessaoResponse>>> GetAll()
        {
            try
            {
                var registros = await _registroService.GetAllSessaoAsync();
                return Ok(registros);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar registros", details = ex.Message });
            }
        }

        /// <summary>
        /// Registra entrada de um promotor
        /// </summary>
        [HttpPost("entrada")]
        public async Task<ActionResult<RegistroResponse>> RegistrarEntrada([FromBody] RegistrarEntradaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var usuarioId = int.TryParse(usuarioIdClaim, out var claimId) ? claimId : request.UsuarioId;

                var registro = await _registroService.RegistrarEntradaAsync(
                    request.PromotorId,
                    request.EmpresaId,
                    usuarioId,
                    request.Observacao);
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
        public async Task<ActionResult<RegistroResponse>> RegistrarSaida([FromBody] RegistrarSaidaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var usuarioId = int.TryParse(usuarioIdClaim, out var claimId) ? claimId : request.UsuarioId;

                var registro = await _registroService.RegistrarSaidaAsync(
                    request.PromotorId,
                    request.EmpresaId,
                    usuarioId,
                    request.Observacao);
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
        /// Registra saída usando o ID do registro de entrada aberto
        /// </summary>
        [HttpPost("{entradaId}/saida")]
        public async Task<ActionResult<RegistroResponse>> RegistrarSaidaPorId(int entradaId, [FromBody] RegistrarSaidaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var usuarioId = int.TryParse(usuarioIdClaim, out var claimId) ? claimId : request.UsuarioId;

                var registro = await _registroService.RegistrarSaidaPorRegistroIdAsync(
                    entradaId,
                    usuarioId,
                    request.Observacao);
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
        public async Task<ActionResult<List<PromotorAtivoResponse>>> GetPromotoresAtivos()
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
        public async Task<ActionResult<List<RegistroResponse>>> GetRegistrosByPromotor(
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
        public async Task<ActionResult<List<RegistroResponse>>> GetRegistrosByEmpresa(
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
        public async Task<ActionResult<RegistroResponse>> GetById(int id)
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
