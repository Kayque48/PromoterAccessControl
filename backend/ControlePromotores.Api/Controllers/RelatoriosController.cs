using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControlePromotores.Api.Services;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RelatoriosController : ControllerBase
    {
        private readonly RelatorioService _relatorioService;

        public RelatoriosController(RelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        /// <summary>
        /// Obtém relatório agregado com filtros
        /// </summary>
        [HttpPost("agregado")]
        public async Task<ActionResult<RelatorioAgregadoResponse>> GetRelatorioAgregado([FromBody] FiltroRelatorioRequest filtro)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var relatorio = await _relatorioService.GetRelatorioAgregadoAsync(filtro);
                return Ok(relatorio);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao gerar relatório", details = ex.Message });
            }
        }

        /// <summary>
        /// Exporta relatório em CSV
        /// </summary>
        [HttpPost("exportar-csv")]
        public async Task<IActionResult> ExportarCSV([FromBody] FiltroRelatorioRequest filtro)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var relatorio = await _relatorioService.ExportarCSVAsync(filtro);
                return File(
                    relatorio.ConteudoArquivo,
                    relatorio.ContentType,
                    relatorio.NomeArquivo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao exportar relatório", details = ex.Message });
            }
        }
    }
}
