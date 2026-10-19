// Criação do Modelo da Categoria dos desafios
namespace PensaAntes.Api.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    public ICollection<Desafio> Desafios { get; set; } = new List<Desafio>();
}