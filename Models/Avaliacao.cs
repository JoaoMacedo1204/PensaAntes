// Criação do Modelo de Avaliação do Sistema
using System.ComponentModel.DataAnnotations;

namespace PensaAntes.Api.Models;

/// <summary>
/// Níveis de cada critério (rubrica do projeto):
/// 0 = Não demonstrado | 1 = Inicial | 2 = Adequado | 3 = Avançado.
/// São níveis de uma rubrica, não são para medir o nível de inteligência.
/// </summary>
public class Avaliacao
{
    public int Id { get; set; }

    public int RespostaId { get; set; }
    public Resposta Resposta { get; set; } = null!;

    [Range(0, 3)] public int Identificacao { get; set; }
    [Range(0, 3)] public int UsoDosDados { get; set; }
    [Range(0, 3)] public int Hipotese { get; set; }
    [Range(0, 3)] public int Justificativa { get; set; }
    [Range(0, 3)] public int PensamentoCritico { get; set; }

    public string Feedback { get; set; } = string.Empty;
    public DateTime DataAvaliacao { get; set; } = DateTime.UtcNow;
}