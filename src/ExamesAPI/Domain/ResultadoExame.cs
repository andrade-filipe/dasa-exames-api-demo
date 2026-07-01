namespace ExamesAPI.Domain;

public class ResultadoExame
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExameId { get; set; }
    public Guid PacienteId { get; set; }

    /// <summary>Valor medido. Nulo enquanto o exame nao tem resultado apurado.</summary>
    public double? Valor { get; set; }
    public string Unidade { get; set; } = string.Empty;

    public StatusResultado Status { get; set; } = StatusResultado.Pendente;

    /// <summary>Momento da liberacao, em UTC.</summary>
    public DateTime? LiberadoEm { get; set; }

    /// <summary>Momento do cancelamento, em UTC.</summary>
    public DateTime? CanceladoEm { get; set; }

    /// <summary>Motivo do cancelamento sem dados sensiveis de paciente.</summary>
    public string? MotivoCancelamento { get; set; }
}
