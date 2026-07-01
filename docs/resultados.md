# Resultados de Exame

## Liberacao
Para liberar um resultado, chame `POST /resultados/{id}/liberar`.

O endpoint retorna:
- `200 OK` quando a liberacao ocorre com sucesso.
- `404 NotFound` quando o resultado nao existe.
- `422 UnprocessableEntity` quando a regra de liberacao e violada.

Regras de liberacao aplicadas pelo servico:
- `Valor` deve estar preenchido (nao nulo).
- Status atual deve ser `Pendente` ou `EmAnalise`.
- `Status` muda para `Liberado` e `LiberadoEm` e preenchido com `DateTime.UtcNow`.

Auditoria:
- Logs de liberacao contem somente identificador tecnico (`ResultadoId`).
- Nome, CPF e valor clinico nao sao registrados em log.