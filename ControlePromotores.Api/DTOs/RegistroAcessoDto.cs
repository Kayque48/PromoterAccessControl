using System.ComponentModel.DataAnnotations;

namespace ControlePromotores.Api.DTOs
{
    public class RegistrarEntradaRequest
    {
        [Required]
        public int PromotorId { get; set; }
    }

    public class RegistrarSaidaRequest
    {
        [Required]
        public int RegistroId { get; set; }
    }

    public class RegistroAcessoResponse
    {
        public int Id { get; set; }
        public DateTime Entrada { get; set; }
        public DateTime? Saida { get; set; }
        public int? TempoPermanenciaMinutos { get; set; }
        public int PromotorId { get; set; }
        public string PromotorNome { get; set; }
    }

    public class PromotorAtativoResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public DateTime Entrada { get; set; }
        public int MinutosAtendimento { get; set; }
    }
}
