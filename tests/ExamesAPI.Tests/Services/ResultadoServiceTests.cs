using ExamesAPI.Domain;
using ExamesAPI.Infra;
using ExamesAPI.Services;
using Microsoft.Extensions.Logging;

namespace ExamesAPI.Tests.Services;

public class ResultadoServiceTests
{
    [Fact]
    public void LiberarResultado_QuandoPendenteComValor_DeveLiberarComUtc()
    {
        // Arrange
        var logger = new LoggerSpy<ResultadoService>();
        var service = CriarServiceComResultado(StatusResultado.Pendente, 4.2, logger, out var resultado);

        // Act
        var resultadoLiberado = service.LiberarResultado(resultado.Id);

        // Assert
        Assert.Equal(StatusResultado.Liberado, resultadoLiberado.Status);
        Assert.NotNull(resultadoLiberado.LiberadoEm);
        Assert.Equal(DateTimeKind.Utc, resultadoLiberado.LiberadoEm!.Value.Kind);
    }

    [Fact]
    public void LiberarResultado_QuandoEmAnaliseComValor_DeveLiberarComUtc()
    {
        // Arrange
        var logger = new LoggerSpy<ResultadoService>();
        var service = CriarServiceComResultado(StatusResultado.EmAnalise, 4.2, logger, out var resultado);

        // Act
        var resultadoLiberado = service.LiberarResultado(resultado.Id);

        // Assert
        Assert.Equal(StatusResultado.Liberado, resultadoLiberado.Status);
        Assert.NotNull(resultadoLiberado.LiberadoEm);
        Assert.Equal(DateTimeKind.Utc, resultadoLiberado.LiberadoEm!.Value.Kind);
    }

    [Fact]
    public void LiberarResultado_QuandoResultadoNaoExiste_DeveLancarResultadoNaoEncontradoException()
    {
        // Arrange
        var repo = new RepositorioMemoria();
        var logger = new LoggerSpy<ResultadoService>();
        var service = new ResultadoService(repo, logger);

        // Act
        var acao = () => service.LiberarResultado(Guid.NewGuid());

        // Assert
        Assert.Throws<ResultadoNaoEncontradoException>(acao);
    }

    [Fact]
    public void LiberarResultado_QuandoValorNulo_DeveLancarRegraLiberacaoException()
    {
        // Arrange
        var logger = new LoggerSpy<ResultadoService>();
        var service = CriarServiceComResultado(StatusResultado.Pendente, null, logger, out var resultado);

        // Act
        var acao = () => service.LiberarResultado(resultado.Id);

        // Assert
        Assert.Throws<RegraLiberacaoException>(acao);
    }

    [Fact]
    public void LiberarResultado_QuandoStatusCancelado_DeveLancarRegraLiberacaoException()
    {
        // Arrange
        var logger = new LoggerSpy<ResultadoService>();
        var service = CriarServiceComResultado(StatusResultado.Cancelado, 4.2, logger, out var resultado);

        // Act
        var acao = () => service.LiberarResultado(resultado.Id);

        // Assert
        Assert.Throws<RegraLiberacaoException>(acao);
    }

    [Fact]
    public void LiberarResultado_QuandoStatusLiberado_DeveLancarRegraLiberacaoException()
    {
        // Arrange
        var logger = new LoggerSpy<ResultadoService>();
        var service = CriarServiceComResultado(StatusResultado.Liberado, 4.2, logger, out var resultado);

        // Act
        var acao = () => service.LiberarResultado(resultado.Id);

        // Assert
        Assert.Throws<RegraLiberacaoException>(acao);
    }

    [Fact]
    public void LiberarResultado_QuandoViolacaoNaoDeveAlterarStatusNemLiberadoEm()
    {
        // Arrange
        var logger = new LoggerSpy<ResultadoService>();
        var service = CriarServiceComResultado(StatusResultado.Cancelado, 4.2, logger, out var resultado);
        var statusOriginal = resultado.Status;
        var liberadoEmOriginal = resultado.LiberadoEm;

        // Act
        Action acao = () => service.LiberarResultado(resultado.Id);

        // Assert
        Assert.Throws<RegraLiberacaoException>(acao);
        Assert.Equal(statusOriginal, resultado.Status);
        Assert.Equal(liberadoEmOriginal, resultado.LiberadoEm);
    }

    [Fact]
    public void LiberarResultado_LogNaoDeveConterNomeCpfOuValor()
    {
        // Arrange
        var logger = new LoggerSpy<ResultadoService>();
        var service = CriarServiceComResultado(StatusResultado.Pendente, 4.2, logger, out var resultado);

        // Act
        _ = service.LiberarResultado(resultado.Id);

        // Assert
        Assert.NotEmpty(logger.Messages);
        var mensagem = logger.Messages.Single();
        Assert.Contains(resultado.Id.ToString(), mensagem);
        Assert.DoesNotContain("Maria", mensagem);
        Assert.DoesNotContain("12345678900", mensagem);
        Assert.DoesNotContain("4.2", mensagem);
    }

    private static ResultadoService CriarServiceComResultado(
        StatusResultado status,
        double? valor,
        LoggerSpy<ResultadoService> logger,
        out ResultadoExame resultado)
    {
        var repo = new RepositorioMemoria();

        var paciente = new Paciente
        {
            Id = Guid.NewGuid(),
            Nome = "Maria",
            Cpf = "12345678900"
        };

        resultado = new ResultadoExame
        {
            Id = Guid.NewGuid(),
            PacienteId = paciente.Id,
            Status = status,
            Valor = valor,
            LiberadoEm = null
        };

        repo.Pacientes.Add(paciente);
        repo.Resultados.Add(resultado);

        return new ResultadoService(repo, logger);
    }

    private sealed class LoggerSpy<T> : ILogger<T>
    {
        public List<string> Messages { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
            => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}