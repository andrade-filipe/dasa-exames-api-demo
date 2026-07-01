# AGENTS.md — ExamesAPI (DASA)

Contexto persistente para agentes de código (Codex CLI) neste repositório.
Serviço .NET de **resultados de exame laboratorial**. Domínio: saúde/diagnósticos.

## Arquitetura

- Camadas: **Controller → Service → Repositório**. Controller não contém regra de negócio.
- Persistência é in-memory (`RepositorioMemoria`) — sem banco nesta demo.
- Nomenclatura do domínio em **pt-BR** (`ResultadoExame`, `LiberarResultado`, `Paciente`).

## Regras de domínio (compliance de saúde — inegociáveis)

1. **Nunca logar nem expor dado sensível de paciente** (CPF, nome, valores clínicos) em texto claro. Logs usam apenas identificadores técnicos (ex.: `ResultadoId`).
2. Um resultado só pode ser **liberado** quando:
   - possui **valor apurado** (`Valor` não nulo); e
   - está em `Pendente` ou `EmAnalise` (não se libera `Cancelado` nem se re-libera `Liberado`).
3. **Timestamps sempre em UTC** (`DateTime.UtcNow`). Nunca hora local.

## Testes

- xUnit, padrão **AAA** (Arrange-Act-Assert).
- Toda regra de negócio em `Services/` deve ter teste cobrindo o caminho feliz e as violações.

## Convenções

- `Nullable` e `ImplicitUsings` habilitados.
- Métodos de serviço pequenos e com responsabilidade única.

## Log da sessão de treino (PROMPTS.md)

Ao receber uma instrução de trabalho do operador (fases Research/Plan/Implement),
acrescente-a verbatim ao final de `PROMPTS.md`, sob um cabeçalho `## <fase/rótulo>`,
antes de agir. É o log que os devs vão reproduzir — não resuma. Crie o arquivo se faltar.
