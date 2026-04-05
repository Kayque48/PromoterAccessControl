using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ControlePromotores.Api.Models;

namespace ControlePromotores.Api.Services
{
    /// <summary>
    /// Serviço responsável pela geração de tokens JWT para autenticação stateless.
    /// Implementa segurança via assinatura com chave simétrica (HMAC-SHA256) conforme RFC 7519.
    /// Tokens contêm claims embarcados (user ID, login, role, nome) para autorização sem consulta ao banco.
    /// </summary>
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GerarToken(Usuario usuario)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var jwtExpirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "1440");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtKey);

            // Claims embarcados: Informações do usuário incluídas no token para autorização sem banco de dados.
            // - NameIdentifier: ID únco do usuário (para auditar quem fez cada ação).
            // - Name: Login (identidade no token).
            // - Role: Perfil do usuário (admin/usuario) para controle de acesso baseado em role (RBAC).
            // - Nome: Nome completo do usuário (para display na UI).
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Login),
                    new Claim(ClaimTypes.Role, usuario.Perfil),
                    new Claim("Nome", usuario.Nome)
                }),
                Expires = DateTime.UtcNow.AddMinutes(jwtExpirationMinutes),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}