using PensaAntes.Api.Models;

namespace PensaAntes.Api.Services;

public interface IAvaliacaoService
{
    /// <summary>
    /// Avalia o raciocínio do usuário com a Gemini. Não grava nada no banco.
    /// Lança HttpRequestException (API respondeu erro) ou InvalidOperationException
    /// (resposta inválida / resposta ainda não finalizada). As mensagens são seguras para log.
    /// </summary>
    Task<ResultadoAvaliacao> AvaliarAsync(
        Desafio desafio,
        Resposta resposta,
        CancellationToken cancellationToken);
}

/// <summary>
/// Resultado já validado: notas inteiras de 0 a 3 e feedback não vazio.
/// </summary>
public sealed record ResultadoAvaliacao(
    int Identificacao,
    int UsoDosDados,
    int Hipotese,
    int Justificativa,
    int PensamentoCritico,
    string Feedback);