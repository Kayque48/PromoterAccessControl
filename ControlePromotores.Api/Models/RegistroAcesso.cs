using System;
using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
    public class RegistroAcesso
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Entrada { get; set; }

        public DateTime? Saida { get; set; }

        public int? TempoPermanenciaMinutos { get; set; }

        [Required]
        public int PromotorId { get; set; }

        public Promotor Promotor { get; set; }
    }
}