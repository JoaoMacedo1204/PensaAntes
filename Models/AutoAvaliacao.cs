//Criação do Modelo da Autoavaliação do Usuário
namespace PensaAntes.Api.Models;

public class AutoAvaliacao
{
    public int Id { get; set; }

    public int RespostaId { get; set; }
    public Resposta Resposta { get; set; } = null!;

    public string OQueFezBem { get; set; } = string.Empty;
    public string OQuePodeMelhorar { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}