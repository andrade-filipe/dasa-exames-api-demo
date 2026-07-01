## Contexto da sessão — 1

Contexto da sessão: este repositório (ExamesAPI) é o palco de um TREINAMENTO. Vamos "assar" a
branch demo rodando o pipeline RPI (Research → Plan → Implement) sobre o código atual, e o
resultado precisa ser 100% reproduzível pelos desenvolvedores.

Regra de log (obrigatória): mantenha um arquivo PROMPTS.md na raiz. ANTES de executar QUALQUER
instrução de trabalho que eu te der a partir de agora, acrescente ao FINAL de PROMPTS.md o meu
prompt VERBATIM, sob um cabeçalho no formato:

## <fase ou rótulo> — <nº da ordem>

  <o prompt exatamente como recebido>

Não resuma, não parafraseie, não invente prompts. Se PROMPTS.md não existir, crie.
Confirme que entendeu essa regra antes de começarmos.

## Research — 2

Use o agente researcher: pesquise o fluxo de liberação de resultado e audite-o contra o
AGENTS.md. Não edite código. Liste os problemas de correção, de compliance e de cobertura de
testes — cada achado com arquivo/linha de evidência. Escreva o resultado em RESEARCH.md.

## Research (Prompt Gerado pelo codex para alimentar o Agente)

Pesquise o fluxo de liberação de resultado no repositório ExamesAPI e audite contra as regras do AGENTS.md. Não planeje nem edite código. Entregue uma lista objetiva de problemas em três categorias: correção funcional, compliance e cobertura de testes, cada item com evidência de arquivo e linha(s). Foque em Controller/Service/Repositorio e testes xUnit relacionados a liberar resultado.

## Plan — 3

Use o agente planner: leia RESEARCH.md e produza um plano cirúrgico e testável em PLAN.md —
quais arquivos tocar, as mudanças exatas, os testes xUnit a criar (AAA) e a estratégia de
verificação. Não edite código.

## Plan (Prompt Gerado pelo codex para alimentar o Agente)

Leia RESEARCH.md e produza PLAN.md com plano cirurgico e testavel para corrigir o fluxo de liberacao. O plano deve listar: arquivos a tocar, mudancas exatas por arquivo, testes xUnit a criar no padrao AAA (caminho feliz + violacoes), e estrategia de verificacao. Nao edite codigo-fonte, apenas escreva PLAN.md.

## Implement — 4

Use o agente implementer: leia PLAN.md e implemente exatamente o que está descrito. Refatore
ResultadoService em métodos pequenos; corrija a liberação (valor obrigatório + estados válidos

- UTC); remova qualquer dado de paciente do log (só identificador técnico); escreva os testes
  xUnit (AAA) cobrindo caminho feliz e as violações; e sincronize docs/resultados.md.
  Ao final, rode `dotnet test` e me mostre o diff completo.

## Implement (Prompt Gerado pelo codex para alimentar o Agente)

Leia PLAN.md e implemente exatamente o plano. Requisitos obrigatorios: refatorar ResultadoService em metodos pequenos; corrigir liberacao (Valor obrigatorio + apenas status Pendente/EmAnalise + timestamp UTC); remover qualquer dado de paciente do log (somente identificador tecnico); escrever testes xUnit em padrao AAA cobrindo caminho feliz e violacoes; sincronizar docs/resultados.md com o comportamento final. Ao terminar, rode dotnet test e inclua no retorno: lista de arquivos alterados e resultado dos testes.

## Security Review — 5

Revise o diff como um Auditor de Segurança: sobrou alguma exposição de dado sensível de
paciente (CPF, nome, valor clínico) em log, retorno de API ou mensagem de erro? Aponte com
arquivo e linha.
