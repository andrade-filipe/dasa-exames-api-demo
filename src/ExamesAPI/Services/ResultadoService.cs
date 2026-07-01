using ExamesAPI.Domain;
using ExamesAPI.Infra;
using System.Globalization;

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

    public ResultadoExame CancelarResultado(Guid resultadoId, string? motivo)
    {
        var resultado = ObterResultadoOuFalhar(resultadoId);
        var motivoNormalizado = ValidarMotivoObrigatorio(motivo);
        ValidarElegibilidadeCancelamento(resultado);
        ValidarMotivoSemDadoSensivel(resultado, motivoNormalizado);
        Cancelar(resultado, motivoNormalizado);
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

    private static string ValidarMotivoObrigatorio(string? motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new RegraCancelamentoException("Motivo de cancelamento obrigatorio.");

        return motivo.Trim();
    }

    private static void ValidarElegibilidadeCancelamento(ResultadoExame resultado)
    {
        if (resultado.Status is not StatusResultado.Pendente and not StatusResultado.EmAnalise)
            throw new RegraCancelamentoException("Somente resultados pendentes ou em analise podem ser cancelados.");
    }

    private void ValidarMotivoSemDadoSensivel(ResultadoExame resultado, string motivo)
    {
        var paciente = _repo.Pacientes.FirstOrDefault(p => p.Id == resultado.PacienteId);
        if (paciente is null)
            return;

        var motivoContemNome = ContemTexto(motivo, paciente.Nome);
        var motivoContemCpf = ContemTexto(motivo, paciente.Cpf);
        var motivoContemValorClinico = resultado.Valor is not null && ContemValorClinico(motivo, resultado.Valor.Value);

        if (motivoContemNome || motivoContemCpf || motivoContemValorClinico)
            throw new RegraCancelamentoException("Motivo de cancelamento nao pode conter dados sensiveis do paciente.");
    }

    private static bool ContemTexto(string texto, string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return false;

        return texto.Contains(valor, StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContemValorClinico(string motivo, double valorClinico)
    {
        var valorInvariantCulture = valorClinico.ToString(CultureInfo.InvariantCulture);
        var valorCurrentCulture = valorClinico.ToString(CultureInfo.CurrentCulture);

        return ContemTexto(motivo, valorInvariantCulture) || ContemTexto(motivo, valorCurrentCulture);
    }

    private static void Cancelar(ResultadoExame resultado, string motivo)
    {
        resultado.Status = StatusResultado.Cancelado;
        resultado.CanceladoEm = DateTime.UtcNow;
        resultado.MotivoCancelamento = motivo;
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
