using System.ComponentModel.DataAnnotations;


namespace ControlePromotores.Api.Models
{

	public class LoginModel

	{
		[Required]
		public string Login { get; set; }

		[required]
		public string Senha { get; set; }
    }
}
