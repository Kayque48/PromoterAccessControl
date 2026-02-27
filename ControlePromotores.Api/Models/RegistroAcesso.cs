using System;

  namespace ControlePromotores.Api.Models
  {

      public class RegistroAcesso
      {
        public int Id { get; set; }

        public DateTime Entrada { get; set; }
          
        public DateTime? Saida { get; set; }
        
        public int? TempoPermanencia { get; set; }

        public int PromotorId { get; set; }

        public Promotor Promotor { get; set; }
        
      }
  }