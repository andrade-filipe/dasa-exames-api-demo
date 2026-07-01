namespace ExamesAPI.Domain;

public class ResultadoNaoEncontradoException : Exception
{
    public ResultadoNaoEncontradoException(Guid resultadoId)
        : base($"Resultado {resultadoId} não encontrado.")
    {
    }
}
