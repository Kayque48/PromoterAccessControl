// ========================================
// MODELO: PROMOTOR
// ========================================
// Representa um promotor que realiza registros de entrada/saída.
// Cada promotor está associado a uma empresa específica.

namespace PromoterAccessControl.Models
{
    public class Promoter
    {
        /// <summary>Identificador único do promotor</summary>
        public int Id { get; set; }

        /// <summary>Nome completo do promotor</summary>
        public string Name { get; set; }

        /// <summary>ID da empresa à qual este promotor pertence (relacionamento 1:N)</summary>
        public int CompanyId { get; set; }
    }
}