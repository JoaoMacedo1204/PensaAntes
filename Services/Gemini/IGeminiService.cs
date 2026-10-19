using PensaAntes.Api.Models;

namespace PensaAntes.Api.Services;

public interface IGeminiService
{
    Task<string> GerarAnaliseAsync(
        Desafio desafio,
        Resposta resposta,
        CancellationToken cancellationToken);
}