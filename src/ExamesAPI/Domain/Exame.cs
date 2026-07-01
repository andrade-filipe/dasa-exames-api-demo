namespace ExamesAPI.Domain;

public class Exame
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Tipo { get; set; } = string.Empty;
    public DateTime DataColeta { get; set; }
}
