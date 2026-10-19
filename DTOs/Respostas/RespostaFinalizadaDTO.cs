using System.Text.Json.Serialization;
using PensaAntes.Api.Enums;

namespace PensaAntes.Api.DTOs.Respostas;

public class RespostaFinalizadaDto
{
    public int Id { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TipoDecisaoFinal DecisaoFinal { get; init; }

    public string JustificativaFinal { get; init; } = string.Empty;
    public bool ConsultouIA { get; init; }
    public DateTime DataFinalizacao { get; init; }
}