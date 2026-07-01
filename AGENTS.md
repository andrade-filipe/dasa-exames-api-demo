# AGENTS.md — ExamesAPI (DASA)

Contexto persistente para agentes de código (Codex CLI) neste repositório.
Serviço .NET de **resultados de exame laboratorial**. Domínio: saúde/diagnósticos.

## Arquitetura

- Camadas: **Controller -> Service -> Repositorio**. Controller não contém regra de negócio.
- Persistência é in-memory (`RepositorioMemoria`) — sem banco nesta demo.
- Nomenclatura do domínio em **pt-BR** (`ResultadoExame`, `LiberarResultado`, `Paciente`).
- **Controllers nao devem retornar entidades de dominio diretamente** quando houver campos potencialmente sensiveis (ex.: `MotivoCancelamento`, valores clinicos). Use DTO explicito de resposta para controlar exposicao.

## Regras de domínio (compliance de saúde — inegociáveis)

1. **Nunca logar nem expor dado sensível de paciente** (CPF, nome, valores clínicos) em texto claro. Logs usam apenas identificadores técnicos (ex.: `ResultadoId`).
2. Um resultado só pode ser **liberado** quando:
   - possui **valor apurado** (`Valor` não nulo); e
   - está em `Pendente` ou `EmAnalise` (não se libera `Cancelado` nem se re-libera `Liberado`).
3. **Timestamps sempre em UTC** (`DateTime.UtcNow`). Nunca hora local.
4. **Validacoes de compliance devem falhar-fechado**:
   - Se a validacao de dado sensivel depender de dados relacionados (ex.: Paciente) e esses dados nao forem encontrados, a operacao deve ser bloqueada com excecao de dominio.
   - E proibido seguir em frente quando nao for possivel verificar exposicao de dado sensivel.

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

