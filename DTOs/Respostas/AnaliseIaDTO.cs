namespace PensaAntes.Api.DTOs.Respostas;

public class AnaliseIaDto
{
    public int RespostaId { get; init; }
    public string AnaliseIA { get; init; } = string.Empty;
    public bool ConsultouIA { get; init; }
}