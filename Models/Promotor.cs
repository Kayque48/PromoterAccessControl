using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ControlePromotores.Api.Models
{
    public class Promotor
    {
        // Definição da chave primária
        [Key]
        public int Id { get; set; }

        // Propriedades do promotor
        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telefone { get; set; }

        [StringLength(18)]
        public string? Cpf { get; set; }

        [Required]
        [StringLength(30)]
        public string Categoria { get; set; } = "Promotor";

        [StringLength(200)]
        public string? Endereco { get; set; }

        [StringLength(100)]
        public string? DiasPermitidos { get; set; }

        [NotMapped]
        public List<string> DiasPermitidosList
        {
            get
            {
                if (string.IsNullOrEmpty(DiasPermitidos))
                    return new List<string>();

                return DiasPermitidos.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();
            }
            set
            {
                DiasPermitidos = value == null ? string.Empty : string.Join(',', value.Distinct());
            }
        }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public bool Ativo { get; set; } = true;

        // Relacionamento com Empresa
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; } = null!;
        public ICollection<RegistroAcesso> Registros { get; set; } = new List<RegistroAcesso>();
    }
}