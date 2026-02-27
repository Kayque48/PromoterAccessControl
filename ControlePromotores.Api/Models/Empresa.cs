using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

    namespace ControlePromotores.Api.Models
    {
        public class Empresa
        {
            //Definição da chave primária
            [Key]
            public int Id { get; set; }

            // Propriedades da empresa
            [Required]
            [StringLength(100)]
            public string Nome { get; set; }

            [StringLength(20)]
            public string Telefone { get; set; }

            [StringLength(200)]
            public string Endereco { get; set; }

            // Relacionamento com Promotores
            public ICollection<Promotor> Promotores { get; set; }
        }
    }