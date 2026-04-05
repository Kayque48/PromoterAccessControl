using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
	public class LoginModel
	{
		[Required]
		public required string Login { get; set; }

		[Required]
		public required string Senha { get; set; }
	}
}