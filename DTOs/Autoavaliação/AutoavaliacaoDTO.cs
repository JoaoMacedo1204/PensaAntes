namespace PensaAntes.Api.DTOs.Respostas;

public class AutoavaliacaoDto
{
    public int Id { get; init; }
    public int RespostaId { get; init; }
    public string OQueFezBem { get; init; } = string.Empty;
    public string OQuePodeMelhorar { get; init; } = string.Empty;
    public DateTime DataCriacao { get; init; }
}