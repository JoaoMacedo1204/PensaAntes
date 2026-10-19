using System.ComponentModel.DataAnnotations;

public class CriarRespostaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "UsuarioId deve ser um número positivo.")]
    public int UsuarioId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "DesafioId deve ser um número positivo.")]
    public int DesafioId { get; set; }

    [Required]
    [StringLength(5000)]
    public string AnaliseInicial { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string JustificativaInicial { get; set; } = string.Empty;
}