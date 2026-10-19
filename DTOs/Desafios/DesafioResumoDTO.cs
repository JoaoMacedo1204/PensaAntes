// DTO que apresenta somente o Título, Descrição, Dificuldade e Categoria do desafio

using System.Text.Json.Serialization;
using PensaAntes.Api.Enums;

namespace PensaAntes.Api.DTOs.Desafios;

public class DesafioResumoDto
{
    public int Id { get; init; }
    public string Titulo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NivelDificuldade Dificuldade { get; init; }

    public string Categoria { get; init; } = string.Empty;
}