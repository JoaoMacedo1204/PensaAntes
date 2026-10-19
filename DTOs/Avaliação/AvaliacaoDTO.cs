namespace PensaAntes.Api.DTOs.Respostas;

public class AvaliacaoDto
{
    public int RespostaId { get; init; }
    public int Identificacao { get; init; }
    public int UsoDosDados { get; init; }
    public int Hipotese { get; init; }
    public int Justificativa { get; init; }
    public int PensamentoCritico { get; init; }

    /// <summary>
    /// Soma dos cinco critérios, de 0 a 15.
    /// </summary>
    public int Total { get; init; }

    public string Feedback { get; init; } = string.Empty;
}