// Criação do Modelo do Usuário
namespace PensaAntes.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AreaPreferencia { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<Resposta> Respostas { get; set; } = new List<Resposta>();
}