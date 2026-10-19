using System.Text.Json.Serialization;
using PensaAntes.Api.Enums;

namespace PensaAntes.Api.DTOs.Desafios;

public class DesafioDetalheDto
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public string SituacaoProblema { get; init; } = string.Empty;
    public string DadosDisponiveis { get; init; } = string.Empty;
    public string Objetivo { get; init; } = string.Empty;
    public string PerguntaPrincipal { get; init; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NivelDificuldade Dificuldade { get; init; }

    public string Categoria { get; init; } = string.Empty;
}