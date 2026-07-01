Achados (Segurança)

1. Alto: exposição de valor clínico no retorno da API

- /C:/Users/Filipe/Repos/dasa-exames-api-demo/src/ExamesAPI/Controllers/ResultadosController.cs:22 retorna IEnumerable<ResultadoExame> diretamente (Listar()), e o modelo inclui Valor.

- /C:/Users/Filipe/Repos/dasa-exames-api-demo/src/ExamesAPI/Controllers/ResultadosController.cs:30 retorna Ok(resultado) no Liberar, também com ResultadoExame.

- /C:/Users/Filipe/Repos/dasa-exames-api-demo/src/ExamesAPI/Domain/ResultadoExame.cs:10 define Valor (dado clínico sensível) no payload.

Sem achados de exposição residual

- Log: sanitizado para identificador técnico apenas em /C:/Users/Filipe/Repos/dasa-exames-api-demo/src/ExamesAPI/Services/ResultadoService.cs:53.
- Mensagem de erro: não contém CPF/nome/valor em /C:/Users/Filipe/Repos/dasa-exames-api-demo/src/ExamesAPI/Domain/
  ResultadoNaoEncontradoException.cs:6, /C:/Users/Filipe/Repos/dasa-exames-api-demo/src/ExamesAPI/Services/ResultadoService.cs:39, /C:/Users/Filipe/
  Repos/dasa-exames-api-demo/src/ExamesAPI/Services/ResultadoService.cs:42, /C:/Users/Filipe/Repos/dasa-exames-api-demo/src/ExamesAPI/Controllers/
  ResultadosController.cs:34, /C:/Users/Filipe/Repos/dasa-exames-api-demo/src/ExamesAPI/Controllers/ResultadosController.cs:38.
