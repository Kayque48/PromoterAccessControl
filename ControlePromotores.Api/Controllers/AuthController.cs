using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.Models;
using ControlePromotores.Api.Services;
using ControlePromotores.Api.DTOs;

namespace ControlePromotores.Api.Controllers
{
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
        /// Realiza login com usuário e senha
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Login == request.Login && u.Ativo);

                if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
                    return Unauthorized(new { message = "Login ou senha inválidos" });

                usuario.UltimoLogin = DateTime.UtcNow;
                _context.Usuarios.Update(usuario);
                await _context.SaveChangesAsync();

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
        /// Registra um novo usuário (apenas para admin)
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] CriarUsuarioRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (await _context.Usuarios.AnyAsync(u => u.Login == request.Login))
                    return BadRequest(new { message = "Login já existe" });

                var usuario = new Usuario
                {
                    Nome = request.Nome,
                    Login = request.Login,
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
                    Perfil = request.Perfil ?? "usuario",
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow
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
        public string Nome { get; set; }
        public string Login { get; set; }
        public string Senha { get; set; }
        public string Perfil { get; set; }
    }
}
