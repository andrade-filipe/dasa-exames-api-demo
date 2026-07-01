using ExamesAPI.Domain;
using ExamesAPI.Infra;
using Microsoft.AspNetCore.Mvc;

namespace ExamesAPI.Controllers;

[ApiController]
[Route("pacientes")]
public class PacientesController : ControllerBase
{
    private readonly RepositorioMemoria _repo;

    public PacientesController(RepositorioMemoria repo) => _repo = repo;

    [HttpGet]
    public IEnumerable<Paciente> Listar() => _repo.Pacientes;

    [HttpPost]
    public ActionResult<Paciente> Criar(Paciente paciente)
    {
        // TODO: cadastro aceita qualquer coisa — sem validação de CPF nem de nome.
        _repo.Pacientes.Add(paciente);
        return Ok(paciente);
    }
}
