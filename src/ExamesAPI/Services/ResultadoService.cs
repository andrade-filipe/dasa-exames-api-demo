using ExamesAPI.Domain;
using ExamesAPI.Infra;

namespace ExamesAPI.Services;

public class ResultadoService
{
    private readonly RepositorioMemoria _repo;
    private readonly ILogger<ResultadoService> _logger;

    public ResultadoService(RepositorioMemoria repo, ILogger<ResultadoService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // TODO(legado): este método cresceu demais ao longo do tempo. Mistura busca,
    // validação, liberação, auditoria e notificação num bloco só. Ninguém mexe com confiança.
    public ResultadoExame LiberarResultado(Guid resultadoId)
    {
        var r = _repo.Resultados.FirstOrDefault(x => x.Id == resultadoId);
        if (r == null)
            throw new InvalidOperationException("Resultado não encontrado");

        var paciente = _repo.Pacientes.FirstOrDefault(p => p.Id == r.PacienteId);

        // Libera o resultado.
        r.Status = StatusResultado.Liberado;
        r.LiberadoEm = DateTime.Now;

        // Auditoria.
        _logger.LogInformation(
            "Resultado {Id} do paciente {Nome} (CPF {Cpf}) liberado com valor {Valor}",
            r.Id, paciente?.Nome, paciente?.Cpf, r.Valor);

        // Notifica interessados.
        Notificar(paciente, r);

        return r;
    }

    private void Notificar(Paciente? paciente, ResultadoExame r)
    {
        // stub — enviaria e-mail / webhook ao paciente e ao médico solicitante.
    }
}
