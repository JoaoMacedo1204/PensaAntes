using System.ComponentModel.DataAnnotations;

namespace PensaAntes.Api.DTOs.Usuarios;

public class CriarUsuarioDto
{
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    public string? AreaPreferencia { get; set; }
}