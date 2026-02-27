using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.Models;
using ControlePromotores.Api.Services;

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
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Login == request.Login);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            {
                return Unauthorized(new { message = "Login ou senha inválidos" });
            }
            var token = _tokenService.GerarToken(usuario);
            return Ok(new { token });
        }
    }
    public class LoginRequest
    {
        public string Login { get; set; }
        public string Senha { get; set; }
    }
    usuario = new
                {
                    usuario.Id,
                    usuario.Nome,
                    usuario.Perfil
    }
}