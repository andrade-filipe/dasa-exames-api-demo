# RESEARCH

## 1. Goal
Mapear o repositorio `C:\Users\Filipe\Repos\dasa-exames-api-demo` e documentar, com evidencia em arquivos reais, a arquitetura atual, o fluxo principal, as regras de dominio implementadas (e nao implementadas), os pontos de teste existentes e os riscos/gaps para evolucao segura.

## 2. Impacted files
Arquivos criticos para entendimento e possiveis mudancas relacionadas ao fluxo de resultados:

- `src/ExamesAPI/Program.cs` (bootstrap da API, DI, roteamento de controllers e seed inicial).
- `src/ExamesAPI/Controllers/ResultadosController.cs` (endpoint de liberacao e listagem de resultados).
- `src/ExamesAPI/Services/ResultadoService.cs` (regra de negocio central de liberacao).
- `src/ExamesAPI/Infra/RepositorioMemoria.cs` (persistencia in-memory compartilhada por controllers/servico).
- `src/ExamesAPI/Infra/SeedData.cs` (dados iniciais que influenciam comportamento da demo).
- `src/ExamesAPI/Domain/ResultadoExame.cs` (estado e campos de negocio do resultado).
- `src/ExamesAPI/Domain/StatusResultado.cs` (estados validos de resultado).
- `src/ExamesAPI/Domain/Paciente.cs` (dados sensiveis presentes no dominio e usados no log).
- `src/ExamesAPI/Controllers/PacientesController.cs` (entrada de dados de paciente sem validacao).
- `tests/ExamesAPI.Tests/UnitTest1.cs` (cobertura atual real).
- `tests/ExamesAPI.Tests/ExamesAPI.Tests.csproj` (stack de testes/coverage).
- `src/ExamesAPI/ExamesAPI.csproj` (target framework e dependencias da API).
- `src/ExamesAPI/obj/project.assets.json` (framework references efetivas restauradas).
- `docs/resultados.md` (documentacao funcional atualmente desatualizada).
- `README.md` (contexto do repositorio e problemas plantados).

## 3. Symbols & logic
Fluxo principal (resultado):

1. `Program` configura DI e pipeline HTTP:
- `builder.Services.AddSingleton<RepositorioMemoria>()` em `src/ExamesAPI/Program.cs`.
- `builder.Services.AddScoped<ResultadoService>()` em `src/ExamesAPI/Program.cs`.
- `app.MapControllers()` em `src/ExamesAPI/Program.cs`.
- `SeedData.Popular(...)` em `src/ExamesAPI/Program.cs`.

2. Endpoint de liberacao:
- `ResultadosController.Liberar(Guid id)` em `src/ExamesAPI/Controllers/ResultadosController.cs` recebe `POST /resultados/{id}/liberar` e delega para `_service.LiberarResultado(id)`.

3. Regra executada hoje no servico:
- `ResultadoService.LiberarResultado(Guid resultadoId)` em `src/ExamesAPI/Services/ResultadoService.cs`:
  - busca resultado por `Id` em `_repo.Resultados`.
  - se nao encontrar, lanca `InvalidOperationException("Resultado nao encontrado")`.
  - busca paciente por `PacienteId`.
  - define `r.Status = StatusResultado.Liberado`.
  - define `r.LiberadoEm = DateTime.Now`.
  - registra log com `Id`, `Nome`, `Cpf`, `Valor`.
  - chama `Notificar(paciente, r)` (stub sem efeito).

4. Modelo de dados de dominio:
- `ResultadoExame` em `src/ExamesAPI/Domain/ResultadoExame.cs` contem `Valor` nullable, `Status` default `Pendente`, `LiberadoEm` documentado como UTC.
- Estados possiveis em `StatusResultado` (`Pendente`, `EmAnalise`, `Liberado`, `Cancelado`) em `src/ExamesAPI/Domain/StatusResultado.cs`.
- `Paciente` contem `Nome` e `Cpf` em `src/ExamesAPI/Domain/Paciente.cs`.

5. Fluxo de pacientes:
- `PacientesController.Criar(Paciente paciente)` em `src/ExamesAPI/Controllers/PacientesController.cs` adiciona direto em memoria e possui TODO explicito de ausencia de validacao de nome/CPF.

## 4. Dependencies
Dependencias internas (cadeia de chamadas):

- HTTP `POST /resultados/{id}/liberar`
  -> `ResultadosController.Liberar` (`src/ExamesAPI/Controllers/ResultadosController.cs`)
  -> `ResultadoService.LiberarResultado` (`src/ExamesAPI/Services/ResultadoService.cs`)
  -> `RepositorioMemoria.Resultados/Pacientes` (`src/ExamesAPI/Infra/RepositorioMemoria.cs`).

- Inicializacao da aplicacao:
  -> `Program` (`src/ExamesAPI/Program.cs`)
  -> `SeedData.Popular` (`src/ExamesAPI/Infra/SeedData.cs`)
  -> carga de `Paciente`, `Exame`, `ResultadoExame` inicial sem valor.

Dependencias externas (NuGet/framework):

- API Web:
  - `Microsoft.NET.Sdk.Web` no projeto `src/ExamesAPI/ExamesAPI.csproj`.
  - framework reference efetiva `Microsoft.AspNetCore.App` em `src/ExamesAPI/obj/project.assets.json`.

- Testes:
  - `xunit` 2.9.3 em `tests/ExamesAPI.Tests/ExamesAPI.Tests.csproj`.
  - `xunit.runner.visualstudio` 3.1.4 em `tests/ExamesAPI.Tests/ExamesAPI.Tests.csproj`.
  - `Microsoft.NET.Test.Sdk` 17.14.1 em `tests/ExamesAPI.Tests/ExamesAPI.Tests.csproj`.
  - `coverlet.collector` 6.0.4 em `tests/ExamesAPI.Tests/ExamesAPI.Tests.csproj`.

## 5. Findings & risks
Observacoes objetivas com risco associado:

1. Violacao de compliance de dados sensiveis no log.
- Evidencia: `ResultadoService.LiberarResultado` loga `Nome`, `Cpf` e `Valor` em `src/ExamesAPI/Services/ResultadoService.cs`.
- Risco: exposicao de PII e dado clinico em trilhas de auditoria/aplicacao.

2. Regra de liberacao esta permissiva demais.
- Evidencia: `ResultadoService.LiberarResultado` nao valida `Valor` nem estado atual antes de setar `Status = Liberado` em `src/ExamesAPI/Services/ResultadoService.cs`.
- Evidencia complementar: `StatusResultado` inclui estados que deveriam restringir transicao (`Liberado`, `Cancelado`) em `src/ExamesAPI/Domain/StatusResultado.cs`.
- Risco: libera resultado sem valor e permite transicoes invalidas (ex.: de `Cancelado` para `Liberado` ou reliberacao).

3. Timestamp de liberacao em hora local, divergente do contrato de dominio.
- Evidencia: uso de `DateTime.Now` em `src/ExamesAPI/Services/ResultadoService.cs`.
- Evidencia complementar: comentario de `LiberadoEm` exige UTC em `src/ExamesAPI/Domain/ResultadoExame.cs`.
- Risco: inconsistencias temporais entre ambientes/timezones e auditoria.

4. Dados seed ja induzem cenario critico de regra faltante.
- Evidencia: seed cria `ResultadoExame` com `Valor = null` e `Status = Pendente` em `src/ExamesAPI/Infra/SeedData.cs`.
- Risco: endpoint de liberacao consegue produzir estado invalido logo no cenario inicial da demo.

5. Cobertura de testes de negocio inexistente para o servico principal.
- Evidencia: `tests/ExamesAPI.Tests/UnitTest1.cs` contem apenas smoke test (`Assert.True(true)`).
- Evidencia complementar: TODO explicito sobre falta de cobertura em `tests/ExamesAPI.Tests/UnitTest1.cs`.
- Risco: regressao silenciosa em regra de negocio/compliance.

6. Endpoint de cadastro de paciente sem validacao.
- Evidencia: TODO em `src/ExamesAPI/Controllers/PacientesController.cs` indicando ausencia de validacao de CPF/nome; metodo adiciona entidade diretamente ao repositorio.
- Risco: entrada de dados invalidos e aumento de superficie de problemas de qualidade/compliance.

7. Deriva documental entre implementacao e docs funcionais.
- Evidencia: `docs/resultados.md` marca explicitamente a documentacao como desatualizada e nao cita pre-condicoes de liberacao nem UTC.
- Evidencia complementar: `README.md` lista os problemas plantados (liberacao sem valor/qualquer status, DateTime.Now, log com CPF/nome, falta de testes).
- Risco: time operar com regra errada, onboarding e manutencao guiados por informacao incorreta.
