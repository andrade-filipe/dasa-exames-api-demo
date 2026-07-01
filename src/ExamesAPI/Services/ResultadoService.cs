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

    public ResultadoExame LiberarResultado(Guid resultadoId)
    {
        var resultado = ObterResultadoOuFalhar(resultadoId);
        ValidarElegibilidadeLiberacao(resultado);
        Liberar(resultado);
        RegistrarAuditoria(resultado);
        Notificar(resultado);
        return resultado;
    }

    public ResultadoExame CancelarResultado(Guid resultadoId)
    {
        var resultado = ObterResultadoOuFalhar(resultadoId);
        ValidarElegibilidadeCancelamento(resultado);
        Cancelar(resultado);
        RegistrarAuditoriaCancelamento(resultado);
        return resultado;
    }

    private ResultadoExame ObterResultadoOuFalhar(Guid resultadoId)
    {
        var resultado = _repo.Resultados.FirstOrDefault(x => x.Id == resultadoId);
        if (resultado is null)
            throw new ResultadoNaoEncontradoException(resultadoId);

        return resultado;
    }

    private static void ValidarElegibilidadeLiberacao(ResultadoExame resultado)
    {
        if (resultado.Valor is null)
            throw new RegraLiberacaoException("Resultado sem valor apurado nao pode ser liberado.");

        if (resultado.Status is not StatusResultado.Pendente and not StatusResultado.EmAnalise)
            throw new RegraLiberacaoException("Somente resultados pendentes ou em analise podem ser liberados.");
    }

    private static void Liberar(ResultadoExame resultado)
    {
        resultado.Status = StatusResultado.Liberado;
        resultado.LiberadoEm = DateTime.UtcNow;
    }

    private static void ValidarElegibilidadeCancelamento(ResultadoExame resultado)
    {
        if (resultado.Status is not StatusResultado.Pendente and not StatusResultado.EmAnalise)
            throw new RegraCancelamentoException("Somente resultados pendentes ou em analise podem ser cancelados.");
    }

    private static void Cancelar(ResultadoExame resultado)
    {
        resultado.Status = StatusResultado.Cancelado;
        resultado.LiberadoEm = null;
    }

    private void RegistrarAuditoria(ResultadoExame resultado)
    {
        _logger.LogInformation("Resultado {ResultadoId} liberado.", resultado.Id);
    }

    private void RegistrarAuditoriaCancelamento(ResultadoExame resultado)
    {
        _logger.LogInformation("Resultado {ResultadoId} cancelado.", resultado.Id);
    }

    private void Notificar(ResultadoExame resultado)
    {
        var paciente = _repo.Pacientes.FirstOrDefault(p => p.Id == resultado.PacienteId);
        _ = paciente;
        // stub - enviaria e-mail / webhook ao paciente e ao medico solicitante.
    }
}
