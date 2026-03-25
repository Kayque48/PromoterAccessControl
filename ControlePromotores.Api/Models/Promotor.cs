using System.Collections.Generic;
using System.componentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
    public class Promotor
    {
        //Definição da chave primária
        [Key]
        public int Id { get; set; }

        // Propriedades do promotor
        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [StringLength(20)]
        public string Telefone { get; set; }

        [StringLength(200)]
        public string Endereco { get; set; }

        // Relacionamento com Empresa
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }
    }
}