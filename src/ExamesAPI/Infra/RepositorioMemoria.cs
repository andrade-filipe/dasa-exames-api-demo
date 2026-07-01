using ExamesAPI.Domain;

namespace ExamesAPI.Infra;

/// <summary>Repositório in-memory — sem banco. Suficiente para a demo.</summary>
public class RepositorioMemoria
{
    public List<Paciente> Pacientes { get; } = new();
    public List<Exame> Exames { get; } = new();
    public List<ResultadoExame> Resultados { get; } = new();
}
