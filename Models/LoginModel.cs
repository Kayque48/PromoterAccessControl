using System.ComponentModel.DataAnnotations;


namespace ControlePromotores.Api.Models
{

    public class LoginModel
    {
        [Required]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string Senha { get; set; } = string.Empty;
    }
}
