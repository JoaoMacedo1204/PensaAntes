namespace PensaAntes.Api.DTOs.Usuarios;

public class UsuarioDto
{
    public int Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? AreaPreferencia { get; init; }
    public DateTime DataCriacao { get; init; }
}