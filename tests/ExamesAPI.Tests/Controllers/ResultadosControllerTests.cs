using ExamesAPI.Controllers;
using ExamesAPI.Domain;
using ExamesAPI.Infra;
using ExamesAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace ExamesAPI.Tests.Controllers;

public class ResultadosControllerTests
{
    [Fact]
    public void Liberar_QuandoServiceRetornaResultado_DeveRetornarOkComPayload()
    {
        // Arrange
        var repo = new RepositorioMemoria();
        var resultado = AdicionarResultado(repo, StatusResultado.Pendente, 10.5);
        var service = new ResultadoService(repo, NullLogger<ResultadoService>.Instance);
        var controller = new ResultadosController(repo, service);

        // Act
        var resposta = controller.Liberar(resultado.Id);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resposta.Result);
        var payload = Assert.IsType<ResultadoExame>(ok.Value);
        Assert.Equal(resultado.Id, payload.Id);
        Assert.Equal(StatusResultado.Liberado, payload.Status);
    }

    [Fact]
    public void Liberar_QuandoResultadoNaoEncontrado_DeveRetornarNotFound()
    {
        // Arrange
        var repo = new RepositorioMemoria();
        var service = new ResultadoService(repo, NullLogger<ResultadoService>.Instance);
        var controller = new ResultadosController(repo, service);

        // Act
        var resposta = controller.Liberar(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundObjectResult>(resposta.Result);
    }

    [Fact]
    public void Liberar_QuandoViolacaoRegraLiberacao_DeveRetornarUnprocessableEntity()
    {
        // Arrange
        var repo = new RepositorioMemoria();
        var resultado = AdicionarResultado(repo, StatusResultado.Cancelado, 10.5);
        var service = new ResultadoService(repo, NullLogger<ResultadoService>.Instance);
        var controller = new ResultadosController(repo, service);

        // Act
        var resposta = controller.Liberar(resultado.Id);

        // Assert
        Assert.IsType<UnprocessableEntityObjectResult>(resposta.Result);
    }

    [Fact]
    public void Cancelar_QuandoServiceRetornaResultado_DeveRetornarOkComPayload()
    {
        // Arrange
        var repo = new RepositorioMemoria();
        var resultado = AdicionarResultado(repo, StatusResultado.Pendente, 10.5);
        var service = new ResultadoService(repo, NullLogger<ResultadoService>.Instance);
        var controller = new ResultadosController(repo, service);
        var request = new ResultadosController.CancelarResultadoRequest { Motivo = "Amostra comprometida" };

        // Act
        var resposta = controller.Cancelar(resultado.Id, request);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(resposta.Result);
        var payload = Assert.IsType<ResultadoExame>(ok.Value);
        Assert.Equal(resultado.Id, payload.Id);
        Assert.Equal(StatusResultado.Cancelado, payload.Status);
        Assert.Equal("Amostra comprometida", payload.MotivoCancelamento);
    }

    [Fact]
    public void Cancelar_QuandoResultadoNaoEncontrado_DeveRetornarNotFound()
    {
        // Arrange
        var repo = new RepositorioMemoria();
        var service = new ResultadoService(repo, NullLogger<ResultadoService>.Instance);
        var controller = new ResultadosController(repo, service);
        var request = new ResultadosController.CancelarResultadoRequest { Motivo = "Amostra comprometida" };

        // Act
        var resposta = controller.Cancelar(Guid.NewGuid(), request);

        // Assert
        Assert.IsType<NotFoundObjectResult>(resposta.Result);
    }

    [Fact]
    public void Cancelar_QuandoViolacaoRegraCancelamento_DeveRetornarUnprocessableEntity()
    {
        // Arrange
        var repo = new RepositorioMemoria();
        var resultado = AdicionarResultado(repo, StatusResultado.Cancelado, 10.5);
        var service = new ResultadoService(repo, NullLogger<ResultadoService>.Instance);
        var controller = new ResultadosController(repo, service);
        var request = new ResultadosController.CancelarResultadoRequest { Motivo = "Amostra comprometida" };

        // Act
        var resposta = controller.Cancelar(resultado.Id, request);

        // Assert
        Assert.IsType<UnprocessableEntityObjectResult>(resposta.Result);
    }

    private static ResultadoExame AdicionarResultado(RepositorioMemoria repo, StatusResultado status, double? valor)
    {
        var paciente = new Paciente { Id = Guid.NewGuid(), Nome = "Paciente", Cpf = "00000000000" };
        repo.Pacientes.Add(paciente);

        var resultado = new ResultadoExame
        {
            Id = Guid.NewGuid(),
            PacienteId = paciente.Id,
            Status = status,
            Valor = valor
        };

        repo.Resultados.Add(resultado);
        return resultado;
    }
}
