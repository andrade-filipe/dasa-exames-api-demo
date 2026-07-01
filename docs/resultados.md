# Resultados de Exame

> ⚠️ Documentação desatualizada (proposital, para a demo). Não reflete as regras atuais.

## Liberação
Para liberar um resultado, chame `POST /resultados/{id}/liberar`. O serviço marca o
resultado como `Liberado` e registra o momento da liberação.

<!-- Faltando: regra de valor obrigatório, estados válidos de origem, e que o timestamp é UTC. -->
