// Criação dos Modelos de Resposta
using PensaAntes.Api.Enums;

namespace PensaAntes.Api.Models;

// Documentação da Resposta

/// <summary>
/// Representa uma tentativa de um usuário em um desafio.
/// A Resposta só é criada quando a análise e a justificativa iniciais
/// já foram registradas; por isso esses campos são obrigatórios.
/// </summary>
public class Resposta
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int DesafioId { get; set; }
    public Desafio Desafio { get; set; } = null!;

    // Etapa 1: obrigatória (registrada antes de qualquer consulta à IA)
    public string AnaliseInicial { get; set; } = string.Empty;
    public string JustificativaInicial { get; set; } = string.Empty;

    // Etapa 2: opcional (só preenchida se o usuário consultar a IA)
    public bool ConsultouIA { get; set; }
    public string? AnaliseIA { get; set; }

    // Etapa 3: preenchida ao finalizar
    public TipoDecisaoFinal? DecisaoFinal { get; set; }
    public string? JustificativaFinal { get; set; }

    public DateTime DataInicio { get; set; } = DateTime.UtcNow;
    public DateTime? DataFinalizacao { get; set; }

    public Avaliacao? Avaliacao { get; set; }
    public AutoAvaliacao? AutoAvaliacao { get; set; }
}