namespace ExamesAPI.Domain;

public class ResultadoExame
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExameId { get; set; }
    public Guid PacienteId { get; set; }

    /// <summary>Valor medido. Nulo enquanto o exame não tem resultado apurado.</summary>
    public double? Valor { get; set; }
    public string Unidade { get; set; } = string.Empty;

    public StatusResultado Status { get; set; } = StatusResultado.Pendente;

    /// <summary>Momento da liberação, em UTC.</summary>
    public DateTime? LiberadoEm { get; set; }
}
