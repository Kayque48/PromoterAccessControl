using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string Login { get; set; }

        [Required]
        public string Senha { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public int UsuarioId { get; set; }
        public string Nome { get; set; }
        public string Perfil { get; set; }
    }
}
