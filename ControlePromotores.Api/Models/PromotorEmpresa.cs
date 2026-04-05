using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.Models
{
    public class PromotorEmpresa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PromotorId { get; set; }
        public Promotor Promotor { get; set; }

        [Required]
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }

        // Dias permitidos: bitmask (1=Dom, 2=Seg, 4=Ter, 8=Qua, 16=Qui, 32=Sex, 64=Sáb)
        // Default: 62 = Seg-Sex
        public byte DiasPermitidos { get; set; } = 62;

        public bool Ativo { get; set; } = true;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
