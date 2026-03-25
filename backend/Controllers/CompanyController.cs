using Microsoft.AspNetCore.Mvc;

// ========================================
// CONTROLLER: EMPRESAS
// ========================================
// Gerencia operações CRUD de empresas.
// Cada empresa é uma organização que contém promotores.
// Endpoints: GET /api/company, POST /api/company

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    // Armazenamento temporário de empresas (em produção, usar banco de dados)
    private static List<Company> companies = new List<Company>();

    /// <summary>
    /// Recupera todas as empresas cadastradas no sistema.
    /// </summary>
    /// <returns>Lista de todas as empresas</returns>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(companies);
    }

    /// <summary>
    /// Registra uma nova empresa no sistema.
    /// </summary>
    /// <param name="company">Dados da empresa a ser criada (Nome obrigatório)</param>
    /// <returns>Dados da empresa criada com ID</returns>
    [HttpPost]
    public IActionResult Create(Company company)
    {
        companies.Add(company);
        return Ok(company);
    }
}