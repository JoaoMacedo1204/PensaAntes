namespace PensaAntes.Api.DTOs.Respostas;

public class RespostaInicialDto
{
    public int Id { get; init; }
    public int UsuarioId { get; init; }
    public int DesafioId { get; init; }
    public string AnaliseInicial { get; init; } = string.Empty;
    public string JustificativaInicial { get; init; } = string.Empty;
    public bool ConsultouIA { get; init; }
    public DateTime DataInicio { get; init; }
}