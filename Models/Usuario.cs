using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
	public class Usuario
	{
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Nome { get; set; } = string.Empty;

		[Required]
		[StringLength(50)]
		public string Login { get; set; } = string.Empty;

		[Required]
		public string SenhaHash { get; set; } = string.Empty;

public string Perfil { get; set; } = "usuario"; // "admin" ou "usuario"
    }
}
