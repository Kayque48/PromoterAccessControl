/// <summary>
/// Registra a entrada e saída de um promotor.
/// Utilizado para controlar a jornada de trabalho e calcular duração de permanência.
/// </summary>
public class AccessLog
{
    /// <summary>Identificador único do registro</summary>
    public int Id { get; set; }

    /// <summary>ID do promotor que está registrando entrada/saída</summary>
    public int PromoterId { get; set; }

    /// <summary>Data e hora do registro de entrada</summary>
    public DateTime EntryTime { get; set; }

    /// <summary>Data e hora do registro de saída (nulo enquanto o promotor estiver dentro do local)</summary>
    public DateTime? ExitTime { get; set; }
}