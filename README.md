# ExamesAPI — repo-exemplo do treino (DASA · Quantum)

API .NET fictícia de **resultados de exame** usada como palco das demos do treinamento de
IA (Codex + Quantum). Domínio de saúde reconhecível, pequeno o bastante para caber na cabeça.

> Nenhum dado real de paciente. Tudo fictício.

## Branches
- **`main`** — código **cru**, com problemas plantados de propósito (ver abaixo). É onde
  roda o **Ask Mode** e o `/skills` ao vivo.
- **`demo`** — o **resultado pré-cozido** de rodar o pipeline (RPI) sobre o `main`:
  `RESEARCH.md`, `PLAN.md`, o diff da correção + refactor, testes xUnit verdes, docs
  sincronizadas, e **`PROMPTS.md`** (log de todos os prompts usados — para os devs verem
  exatamente o que aconteceu). Revele com `git diff main..demo`.

## Problemas plantados (ganchos das demos)
1. `ResultadoService.LiberarResultado` — método legado emaranhado (validação + liberação +
   auditoria + notificação num bloco só).
2. **Bug de liberação:** libera resultado **sem valor**, a partir de **qualquer status**, e
   grava **hora local** (`DateTime.Now`) em vez de UTC.
3. **Compliance:** o log de auditoria expõe **nome e CPF** do paciente (viola o `AGENTS.md`).
4. **Cobertura ~zero:** `ResultadoService` sem testes.
5. **Docs desatualizadas:** `docs/resultados.md` não reflete as regras reais.

## Rodar
```bash
dotnet run --project src/ExamesAPI
dotnet test
```

## Pack instalado
As skills do time (Codex) estão em `.agents/skills/` — rode `/skills` no Codex para ver.

Spec e roteiro: no vault, `2 Areas/Trabalho/Verity/Quantum-Apresentacao/`.
