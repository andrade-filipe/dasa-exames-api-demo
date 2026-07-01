## [dotnet-test-sandbox] Falha ao rodar `dotnet test` por acesso ao NuGet.Config do perfil
**Cause:** Em ambiente sandbox, o caminho de perfil do usuario (`AppData/Roaming/NuGet/NuGet.Config`) pode nao ser legivel.
**Fix:** Tentar primeiro `dotnet test --no-restore` (quando restore previo ja existe no workspace). Se precisar restore, pedir execucao fora do sandbox com aprovacao.
**Location:** pipelines locais de validacao / sessoes Codex CLI.
