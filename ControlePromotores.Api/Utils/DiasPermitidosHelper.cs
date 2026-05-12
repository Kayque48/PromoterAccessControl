namespace ControlePromotores.Api.Utils
{
    /// <summary>
    /// Utilitário para conversão entre representação textual (array de dias) e bitmap (int) dos dias permitidos.
    /// Padrão bitmap: domingo=1, segunda=2, terça=4, quarta=8, quinta=16, sexta=32, sábado=64.
    /// Exemplo: segunda + quarta + sexta = 2 + 8 + 32 = 42.
    /// </summary>
    public static class DiasPermitidosHelper
    {
        /// <summary>
        /// Mapa de dias da semana em português para valores de bitmask.
        /// </summary>
        private static readonly Dictionary<string, int> DiaParaBitmask = new()
        {
            { "domingo", 1 },
            { "segunda", 2 },
            { "terça", 4 },
            { "quarta", 8 },
            { "quinta", 16 },
            { "sexta", 32 },
            { "sábado", 64 }
        };

        /// <summary>
        /// Mapa inverso: bitmask para nome do dia.
        /// </summary>
        private static readonly Dictionary<int, string> BitmaskParaDia = new()
        {
            { 1, "domingo" },
            { 2, "segunda" },
            { 4, "terça" },
            { 8, "quarta" },
            { 16, "quinta" },
            { 32, "sexta" },
            { 64, "sábado" }
        };

        /// <summary>
        /// Converte array de nomes de dias (em português) para valor bitmask (int).
        /// Exemplo: ["segunda", "quarta", "sexta"] → 42
        /// </summary>
        public static int DiaArrayParaBitmask(string[]? dias)
        {
            if (dias == null || dias.Length == 0)
                return 127; // Padrão: todos os dias (1+2+4+8+16+32+64=127)

            int bitmask = 0;
            foreach (var dia in dias)
            {
                var diaLower = dia.ToLower().Trim();
                if (DiaParaBitmask.TryGetValue(diaLower, out var bit))
                    bitmask |= bit; // Operação OR para ativar o bit correspondente
            }

            return bitmask;
        }

        /// <summary>
        /// Converte valor bitmask (int) para array de nomes de dias em português.
        /// Exemplo: 42 → ["segunda", "quarta", "sexta"]
        /// </summary>
        public static string[] BitmaskParaDiaArray(int bitmask)
        {
            var dias = new List<string>();

            foreach (var kvp in BitmaskParaDia)
            {
                if ((bitmask & kvp.Key) != 0) // Verifica se o bit está ativo
                    dias.Add(kvp.Value);
            }

            return dias.OrderBy(d => DiaParaBitmask[d]).ToArray(); // Ordena por ordem de semana
        }

        /// <summary>
        /// Valida se um dia específico é permitido conforme o bitmask.
        /// Exemplo: bitmask=42, dayOfWeek=1 (segunda-feira) → true
        /// </summary>
        public static bool IsDiaPermitido(int bitmask, DayOfWeek dayOfWeek)
        {
            // Converter DayOfWeek (.NET: 0=Sunday, 1=Monday, ...) para bitmap (1=Sunday, 2=Monday, ...)
            int bit = dayOfWeek switch
            {
                DayOfWeek.Sunday => 1,
                DayOfWeek.Monday => 2,
                DayOfWeek.Tuesday => 4,
                DayOfWeek.Wednesday => 8,
                DayOfWeek.Thursday => 16,
                DayOfWeek.Friday => 32,
                DayOfWeek.Saturday => 64,
                _ => 0
            };

            return (bitmask & bit) != 0;
        }
    }
}
