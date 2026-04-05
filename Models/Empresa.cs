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
            [StringLength(18)]
            public string Cnpj { get; set; } = string.Empty;

            [Required]
            [StringLength(150)]
            public string RazaoSocial { get; set; } = string.Empty;

            [StringLength(150)]
            public string? NomeFantasia { get; set; }

            [StringLength(20)]
            public string? Telefone { get; set; }

            [EmailAddress]
            [StringLength(100)]
            public string? EmailCorporativo { get; set; }

            [StringLength(200)]
            public string? Endereco { get; set; }

            // Relacionamento com Promotores
            [System.Text.Json.Serialization.JsonIgnore]
            public ICollection<Promotor> Promotores { get; set; } = new List<Promotor>();
        }
    }