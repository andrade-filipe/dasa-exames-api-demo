using ExamesAPI.Domain;
using ExamesAPI.Infra;
using ExamesAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExamesAPI.Controllers;

[ApiController]
[Route("resultados")]
public class ResultadosController : ControllerBase
{
    private readonly RepositorioMemoria _repo;
    private readonly ResultadoService _service;

    public ResultadosController(RepositorioMemoria repo, ResultadoService service)
    {
        _repo = repo;
        _service = service;
    }

    [HttpGet]
    public IEnumerable<ResultadoExame> Listar() => _repo.Resultados;

    [HttpPost("{id:guid}/liberar")]
    public ActionResult<ResultadoExame> Liberar(Guid id)
    {
        try
        {
            var resultado = _service.LiberarResultado(id);
            return Ok(resultado);
        }
        catch (ResultadoNaoEncontradoException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (RegraLiberacaoException ex)
        {
            return UnprocessableEntity(new { erro = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancelar")]
    public ActionResult<ResultadoExame> Cancelar(Guid id)
    {
        try
        {
            var resultado = _service.CancelarResultado(id);
            return Ok(resultado);
        }
        catch (ResultadoNaoEncontradoException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (RegraCancelamentoException ex)
        {
            return UnprocessableEntity(new { erro = ex.Message });
        }
    }
}
