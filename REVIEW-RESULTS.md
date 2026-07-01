• Findings (ordenado por severidade)

1. BLOCKER — validação de compliance falha em aberto quando paciente não existe no repositório
   Em cancelamento, se PacienteId não for encontrado, o método retorna sem validar motivo sensível (return;), permitindo gravar/expor motivo
   potencialmente sensível.
   Arquivo: src/ExamesAPI/Services/ResultadoService.cs:79, src/ExamesAPI/Services/ResultadoService.cs:80, src/ExamesAPI/Services/
   ResultadoService.cs:81.
   Personas: Saboteur + Security Auditor (promovido).

2. HIGH — regra “motivo nunca conter dado de paciente” está incompleta e facilmente burlável
   A checagem usa Contains literal só para Nome, Cpf e Valor como string; variações comuns (CPF formatado, acentos/normalização, separadores, outros
   identificadores do paciente) passam. Isso viola requisito de compliance “NUNCA”.
   Arquivo: src/ExamesAPI/Services/ResultadoService.cs:83, src/ExamesAPI/Services/ResultadoService.cs:84, src/ExamesAPI/Services/
   ResultadoService.cs:91, src/ExamesAPI/Services/ResultadoService.cs:99.
   Personas: Security Auditor + Saboteur (promovido).

3. MEDIUM — API expõe MotivoCancelamento diretamente no contrato de saída
   Endpoint retorna ResultadoExame (domínio) direto; após incluir MotivoCancelamento, qualquer resposta/listagem de resultados passa a carregar esse
   texto livre. Mesmo com validação, é aumento de superfície de exposição sensível.
   Arquivo: src/ExamesAPI/Controllers/ResultadosController.cs:22, src/ExamesAPI/Controllers/ResultadosController.cs:43, src/ExamesAPI/Domain/
   ResultadoExame.cs:22.
   Personas: Security Auditor + New Hire.

4. MEDIUM — gap de teste no boundary HTTP para motivo inválido
   Serviço testa motivo nulo/branco, mas controller não cobre explicitamente request nulo / motivo vazio no endpoint novo. Para manter padrão robusto
   no boundary, faltam esses casos em controller tests.
   Arquivo: tests/ExamesAPI.Tests/Controllers/ResultadosControllerTests.cs:62.
   Persona: New Hire.

Resumo rápido (agnostic-code-quality)

- KISS/YAGNI: implementação está direta, mas a validação de compliance por Contains literal cria complexidade frágil e não fecha o requisito.
- SRP: ResultadoService acumula regra de estado + sanitização/compliance textual; tende a ficar difícil de manter/auditar sem separar validador de compliance.
