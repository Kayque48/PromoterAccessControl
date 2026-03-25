using Microsoft.AspNetCore.Mvc;
using PromoterAccessControl.Models;

// ========================================
// CONTROLLER: REGISTROS DE ACESSO
// ========================================
// Gerencia entrada e saída de promotores.
// Responsável por registrar horários de trabalho e calcular duração de permanência.
// Endpoints:
//   GET  /api/accesslog        - Listar todos os registros
//   POST /api/accesslog/entry  - Registrar entrada de promotor
//   POST /api/accesslog/exit   - Registrar saída de promotor

namespace PromoterAccessControl.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessLogController : ControllerBase
    {
        // Armazenamento temporário dos registros de acesso (em produção, usar banco de dados)
        private static List<AccessLog> logs = new List<AccessLog>();

        /// <summary>
        /// Recupera todos os registros de entrada/saída do sistema.
        /// </summary>
        /// <returns>Lista completa de registros de acesso</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(logs);
        }

        /// <summary>
        /// Registra a entrada de um promotor no local.
        /// Cria um novo registro com hora de entrada e deixa saída em aberto (null).
        /// </summary>
        /// <param name="promoterId">ID do promotor que está entrando</param>
        /// <returns>Registro de entrada criado com ID e horário</returns>
        [HttpPost("entry")]
        public IActionResult RegisterEntry(int promoterId)
        {
            // Cria novo registro de acesso com entrada marcada e saída pendente
            var log = new AccessLog
            {
                Id = logs.Count + 1,
                PromoterId = promoterId,
                EntryTime = DateTime.Now,
                ExitTime = null  // Saída ainda não registrada
            };

            logs.Add(log);

            return Ok(log);
        }

        /// <summary>
        /// Registra a saída de um promotor e calcula duração da permanência.
        /// Localiza o registro de entrada mais recente sem saída do promotor.
        /// </summary>
        /// <param name="promoterId">ID do promotor que está saindo</param>
        /// <returns>
        /// Registro de acesso completo com horário de entrada, saída e duração em minutos.
        /// Retorna 404 se não houver registro de entrada aberto para o promotor.
        /// </returns>
        [HttpPost("exit")]
        public IActionResult RegisterExit(int promoterId)
        {
            // Busca o registro de entrada mais recente que ainda não tem saída registrada
            // Regra: Um promotor não pode ter duas entradas ativas simultaneamente
            var openLog = logs
                .LastOrDefault(l => l.PromoterId == promoterId && l.ExitTime == null);

            if (openLog == null)
                return NotFound("No open entry found for this promoter.");

            // Marca o horário de saída
            openLog.ExitTime = DateTime.Now;

            // Calcula a duração total da permanência no local
            var duration = openLog.ExitTime.Value - openLog.EntryTime;

            return Ok(new
            {
                openLog.Id,
                openLog.PromoterId,
                openLog.EntryTime,
                openLog.ExitTime,
                // Duração em minutos, arredondada para 2 casas decimais
                Duration = Math.Round(duration.TotalMinutes, 2) + " minutos"
            });
        }
    }
}