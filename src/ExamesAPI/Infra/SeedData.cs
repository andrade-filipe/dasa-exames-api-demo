using ExamesAPI.Domain;

namespace ExamesAPI.Infra;

/// <summary>Popula o repositório com dados fictícios para a demo.</summary>
public static class SeedData
{
    public static void Popular(RepositorioMemoria repo)
    {
        if (repo.Pacientes.Count > 0) return;

        var paciente = new Paciente
        {
            Nome = "Fulano de Tal",
            Cpf = "000.000.000-00",
            DataNascimento = new DateOnly(1990, 1, 1)
        };
        var exame = new Exame { Tipo = "Hemograma", DataColeta = DateTime.UtcNow.AddDays(-1) };

        repo.Pacientes.Add(paciente);
        repo.Exames.Add(exame);

        // Um resultado ainda SEM valor apurado (Status Pendente) — usado na demo.
        repo.Resultados.Add(new ResultadoExame
        {
            ExameId = exame.Id,
            PacienteId = paciente.Id,
            Valor = null,
            Unidade = "mg/dL",
            Status = StatusResultado.Pendente
        });
    }
}
