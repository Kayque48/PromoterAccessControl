using Microsoft.AspNetCore.Mvc;
using PromoterAccessControl.Models;

// ========================================
// CONTROLLER: PROMOTORES
// ========================================
// Gerencia cadastro e consulta de promotores.
// Promotores são os usuários que realizam registros de acesso.
// Endpoints: GET /api/promoter, POST /api/promoter

namespace PromoterAccessControl.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromoterController : ControllerBase
    {
        // Armazenamento temporário de promotores (em produção, usar banco de dados)
        private static List<Promoter> promoters = new List<Promoter>();

        /// <summary>
        /// Recupera todos os promotores cadastrados no sistema.
        /// </summary>
        /// <returns>Lista de todos os promotores</returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(promoters);
        }

        /// <summary>
        /// Registra um novo promotor no sistema.
        /// O ID é gerado automaticamente de forma sequencial.
        /// </summary>
        /// <param name="promoter">Dados do promotor (Nome e CompanyId obrigatórios)</param>
        /// <returns>Dados do promotor criado com ID atribuído</returns>
        [HttpPost]
        public IActionResult Create(Promoter promoter)
        {
            // Atribui ID sequencial baseado na quantidade de promotores existentes
            promoter.Id = promoters.Count + 1;
            promoters.Add(promoter);

            return Ok(promoter);
        }
    }
}