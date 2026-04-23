using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControlePromotores.Api.Services;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Obtém dados do dashboard para hoje
        /// </summary>
        [HttpGet("hoje")]
        public async Task<ActionResult<DashboardResponse>> GetDashboardHoje()
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardHojeAsync();
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar dados do dashboard", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém visitas por dia da última semana
        /// </summary>
        [HttpGet("visitassemana")]
        public async Task<ActionResult<List<VisitasPorDiaResponse>>> GetVisitasPorDiaSemana()
        {
            try
            {
                var visitas = await _dashboardService.GetVisitasPorDiaSemanaAsync();
                return Ok(visitas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar visitas", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém duração média por promotor
        /// </summary>
        [HttpGet("duraomedia")]
        public async Task<ActionResult<List<DuracaoMediaPorPromotorResponse>>> GetDuracaoMediaPorPromotor(
            [FromQuery] int limitePromotores = 10)
        {
            try
            {
                var duracao = await _dashboardService.GetDuracaoMediaPorPromotorAsync(limitePromotores);
                return Ok(duracao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar duração média", details = ex.Message });
            }
        }

        /// <summary>
        /// Obtém ranking de visitas por empresa
        /// </summary>
        [HttpGet("rankingempresa")]
        public async Task<ActionResult<List<RankingVisitasPorEmpresaResponse>>> GetRankingVisitasPorEmpresa()
        {
            try
            {
                var ranking = await _dashboardService.GetRankingVisitasPorEmpresaAsync();
                return Ok(ranking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar ranking", details = ex.Message });
            }
        }
    }
}
