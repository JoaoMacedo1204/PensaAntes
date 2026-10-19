using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using PensaAntes.Api.Enums;

namespace PensaAntes.Api.DTOs.Respostas;

public class FinalizarRespostaDto
{
    [EnumDataType(typeof(TipoDecisaoFinal),
        ErrorMessage = "DecisaoFinal deve ser ManterRespostaInicial ou ModificarComBaseNaIA.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TipoDecisaoFinal DecisaoFinal { get; set; }

    [Required]
    [StringLength(5000)]
    public string JustificativaFinal { get; set; } = string.Empty;
}