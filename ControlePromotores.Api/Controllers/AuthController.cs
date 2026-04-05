using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.Models;
using ControlePromotores.Api.Services;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Controllers
{
    /// <summary>
    /// Controlador responsável por autenticação e emissão de tokens JWT.
    /// Implementa padrão de segurança sem estado (stateless) via tokens portadores.
    /// Valida credenciais contra banco de dados com hash BCrypt de senha.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PromotoresContext _context;
        private readonly TokenService _tokenService;

        public AuthController(PromotoresContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Realiza autenticação do usuário validando credenciais contra banco de dados.
        /// Retorna JWT token assinado para uso em requisições subsequentes.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Busca usuário ativo pelo login (índice UNIQUE garante O(1)).
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Login == request.Login && u.Ativo);

                // Validação dupla: usuário existe + senha corresponde ao hash armazenado.
                // BCrypt.Verify: Compara senha em texto plano com hash usando salt armazenado.
                // Impede timing attacks e força bruta (custo computacional por tentativa).
                if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
                    return Unauthorized(new { message = "Login ou senha inválidos" });

                // Atualiza AtualizadoEm com timestamp do banco para auditoria implícita de último acesso.
                usuario.AtualizadoEm = DateTime.UtcNow;
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

                // Gera token JWT com claims embarcados (sem necessidade de nova consulta ao banco).
                var token = _tokenService.GerarToken(usuario);
                var response = new LoginResponse
                {
                    Token = token,
                    UsuarioId = usuario.Id,
                    Nome = usuario.Nome,
                    Perfil = usuario.Perfil
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao fazer login", details = ex.Message });
            }
        }

        /// <summary>
        /// Registra novo usuário no sistema. Requer autorização adequada (deve ser protegido via [Authorize(Roles = "admin")]).
        /// Valida unicidade de login e armazena senha com hash BCrypt.
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] CriarUsuarioRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Validação de username duplicado: impede dois usuários com mesmo login.
                if (await _context.Usuarios.AnyAsync(u => u.Login == request.Login))
                    return BadRequest(new { message = "Login já existe" });

                // Criação de usuário com senha hasheada via BCrypt.
                // BCrypt.HashPassword: Gera salt aleatório + hash com custo 10 (padrão).
                // Nunca armazena senha em texto plano (violação de LGPD/GDPR).
                var usuario = new Usuario
                {
                    Nome = request.Nome,
                    Login = request.Login,
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                    Perfil = request.Perfil ?? "usuario",
                    Ativo = true
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                var token = _tokenService.GerarToken(usuario);
                return Ok(new LoginResponse
                {
                    Token = token,
                    UsuarioId = usuario.Id,
                    Nome = usuario.Nome,
                    Perfil = usuario.Perfil
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao registrar", details = ex.Message });
            }
        }
    }

    public class CriarUsuarioRequest
    {
        public required string Nome { get; set; }
        public required string Login { get; set; }
        public required string Senha { get; set; }
        public string? Perfil { get; set; }
    }
}
