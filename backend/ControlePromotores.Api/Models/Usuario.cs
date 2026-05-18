using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
	public class Usuario
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Nome { get; set; }

		[Required]
		[StringLength(150)]
		public string Login { get; set; }

		[Required]
		public string SenhaHash { get; set; }

		[StringLength(20)]
		public string Telefone { get; set; }

		[StringLength(80)]
		public string Cargo { get; set; }

		[Required]
		public string Perfil { get; set; } // "admin" ou "usuario"

		public bool Ativo { get; set; } = true;

		public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

		public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

		// Relacionamentos
		public ICollection<Registro> RegistrosRegistrados { get; set; } = new List<Registro>();
	}
}
