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
        var resultado = _service.LiberarResultado(id);
        return Ok(resultado);
    }
}
