using System.ComponentModel.DataAnnotations;

namespace PensaAntes.Api.DTOs.Respostas;

public class CriarAutoavaliacaoDto
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(2000)]
    public string OQueFezBem { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [StringLength(2000)]
    public string OQuePodeMelhorar { get; set; } = string.Empty;
}