# RESEARCH

## 1. Goal
Auditar o fluxo de liberacao de resultado no ExamesAPI (Controller -> Service -> Repositorio) contra as regras do `AGENTS.md`, sem alterar codigo, e listar problemas objetivos em tres categorias: correcao funcional, compliance e cobertura de testes, com evidencia de arquivo e linha.

## 2. Impacted files
- `src/ExamesAPI/Controllers/ResultadosController.cs` (endpoint `POST /resultados/{id}/liberar`, delegacao para service) [linhas 24-29].
- `src/ExamesAPI/Services/ResultadoService.cs` (regra principal de liberacao, log/auditoria, timestamp) [linhas 19-39].
- `src/ExamesAPI/Infra/RepositorioMemoria.cs` (armazenamento in-memory de resultados/pacientes/exames) [linhas 6-10].
- `src/ExamesAPI/Domain/ResultadoExame.cs` (campos `Valor`, `Status`, `LiberadoEm`) [linhas 10, 13, 16].
- `src/ExamesAPI/Domain/StatusResultado.cs` (estados permitidos) [linhas 5-8].
- `src/ExamesAPI/Infra/SeedData.cs` (estado inicial usado na demo, com `Valor = null` e `Pendente`) [linhas 23-31].
- `src/ExamesAPI/Program.cs` (registro DI de `ResultadoService` e `RepositorioMemoria`) [linhas 9-10, 22].
- `tests/ExamesAPI.Tests/UnitTest1.cs` (suite atual de testes; sem testes de regra de liberacao) [linhas 3-11].
- `tests/ExamesAPI.Tests/ExamesAPI.Tests.csproj` (dependencias xUnit/SDK/cobertura) [linhas 11-14, 22].
- `AGENTS.md` (regras de dominio/compliance usadas como criterio de auditoria).

## 3. Symbols & logic
- `ResultadosController.Liberar(Guid id)` chama diretamente `_service.LiberarResultado(id)` e retorna `Ok(resultado)` sem tratamento de excecoes [src/ExamesAPI/Controllers/ResultadosController.cs:25-29].
- `ResultadoService.LiberarResultado(Guid resultadoId)`:
  - busca resultado por id no repositorio [src/ExamesAPI/Services/ResultadoService.cs:21];
  - lanca `InvalidOperationException` se nao encontrado [linha 23];
  - busca paciente por `PacienteId` [linha 25];
  - define `Status = Liberado` incondicionalmente [linha 28];
  - define `LiberadoEm = DateTime.Now` [linha 29];
  - registra log com `Nome`, `Cpf` e `Valor` [linhas 32-34];
  - chama `Notificar(...)` (stub) [linhas 37, 42-45].
- Modelo de dominio:
  - `ResultadoExame.Valor` e nullable [src/ExamesAPI/Domain/ResultadoExame.cs:10];
  - `Status` default `Pendente` [linha 13];
  - `LiberadoEm` documentado como UTC [linha 15-16].
- Estados disponiveis: `Pendente`, `EmAnalise`, `Liberado`, `Cancelado` [src/ExamesAPI/Domain/StatusResultado.cs:5-8].
- Seed da demo cria resultado `Pendente` com `Valor = null` [src/ExamesAPI/Infra/SeedData.cs:23-31], exercitando um caso em que liberacao deveria ser bloqueada pelas regras do AGENTS.

## 4. Dependencies
Internas (cadeia de chamada):
- HTTP `POST /resultados/{id}/liberar` -> `ResultadosController.Liberar` -> `ResultadoService.LiberarResultado` -> `RepositorioMemoria.Resultados/Pacientes` [src/ExamesAPI/Controllers/ResultadosController.cs:24-29; src/ExamesAPI/Services/ResultadoService.cs:21-25; src/ExamesAPI/Infra/RepositorioMemoria.cs:8-10].
- `ResultadoService` depende de `ILogger<ResultadoService>` para auditoria [src/ExamesAPI/Services/ResultadoService.cs:9, 11-14, 32-34].

Externas (NuGet):
- API:
  - `Swashbuckle.AspNetCore` [src/ExamesAPI/ExamesAPI.csproj:10].
- Testes:
  - `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector` [tests/ExamesAPI.Tests/ExamesAPI.Tests.csproj:11-14].

## 5. Findings & risks
### Correcao funcional
- Violacao da pre-condicao de valor apurado: o service libera mesmo com `Valor = null`, pois nao ha validacao antes de setar `Status = Liberado` [src/ExamesAPI/Services/ResultadoService.cs:28] e a seed cria exatamente esse cenario [src/ExamesAPI/Infra/SeedData.cs:28-31].
- Violacao de estados de origem: o service nao restringe liberacao para apenas `Pendente`/`EmAnalise`; qualquer estado vira `Liberado` (inclusive `Cancelado` e ja `Liberado`) [src/ExamesAPI/Services/ResultadoService.cs:28; src/ExamesAPI/Domain/StatusResultado.cs:5-8].
- Contrato HTTP pouco robusto para erro funcional: quando resultado nao existe, o service lanca `InvalidOperationException` [src/ExamesAPI/Services/ResultadoService.cs:23] e o controller nao trata, retornando erro nao mapeado em vez de resposta de dominio controlada [src/ExamesAPI/Controllers/ResultadosController.cs:25-29].

### Compliance
- Exposicao de dado sensivel em log: mensagem inclui `Nome`, `Cpf` e `Valor` em texto claro [src/ExamesAPI/Services/ResultadoService.cs:33-34], contrariando a regra de nunca logar dado sensivel.
- Timestamp fora de UTC: uso de `DateTime.Now` [src/ExamesAPI/Services/ResultadoService.cs:29], apesar de `ResultadoExame.LiberadoEm` documentar UTC [src/ExamesAPI/Domain/ResultadoExame.cs:15-16] e AGENTS exigir `DateTime.UtcNow`.
- Risco adicional de exposicao via GET: `ResultadosController.Listar` retorna entidade completa (`ResultadoExame`) [src/ExamesAPI/Controllers/ResultadosController.cs:21-22]; no modelo atual ela nao traz `Nome/Cpf`, mas o endpoint nao aplica qualquer filtro/redacao para dados clinicos (`Valor`) [src/ExamesAPI/Domain/ResultadoExame.cs:10].

### Cobertura de testes
- Nao existem testes unitarios da regra de liberacao em `ResultadoService`; ha apenas smoke test `Assert.True(true)` [tests/ExamesAPI.Tests/UnitTest1.cs:3-11].
- Ausencia de testes de caminho feliz e de violacoes obrigatorias descritas no AGENTS (valor nulo, status invalido, re-liberacao, cancelado, UTC em `LiberadoEm`, higienizacao de log) [tests/ExamesAPI.Tests/UnitTest1.cs:3-11].
- Embora o projeto de testes tenha dependencias xUnit/cobertura configuradas [tests/ExamesAPI.Tests/ExamesAPI.Tests.csproj:11-14], nao ha evidencia de cenarios cobrindo Controller/Service/Repositorio no fluxo de liberar resultado [tests/ExamesAPI.Tests/UnitTest1.cs:3-11].
