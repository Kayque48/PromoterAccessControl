using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.DTOs
{
    public class LoginRequest
    {
        [Required]
        public required string Login { get; set; }

        [Required]
        public required string Senha { get; set; }
    }

    public class LoginResponse
    {
        public required string Token { get; set; }
        public int UsuarioId { get; set; }
        public required string Nome { get; set; }
        public required string Perfil { get; set; }
    }
}
