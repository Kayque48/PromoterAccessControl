using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
	public class Usuario
	{
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Nome { get; set; }

		[Required]
		[StringLength(50)]
		public string Login { get; set; }

		[Required]
		public string SenhaHash { get; set; }

		public string Perfil { get; set; } //"admin" ou "usuario"
    }
	
	
}
