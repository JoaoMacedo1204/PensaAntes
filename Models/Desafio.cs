// Criação dos Modelos do Desafio

using PensaAntes.Api.Enums;

namespace PensaAntes.Api.Models;

public class Desafio
{
    public int Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string SituacaoProblema { get; set; } = string.Empty;

    // Texto livre por enquanto (sem modelo separado para cada dado).
    public string DadosDisponiveis { get; set; } = string.Empty;

    public string Objetivo { get; set; } = string.Empty;

    public string PerguntaPrincipal { get; set; } = string.Empty;

    public NivelDificuldade Dificuldade { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public int CategoriaId { get; set; }

    public Categoria Categoria { get; set; } = null!;

    public ICollection<Resposta> Respostas { get; set; } = new List<Resposta>();
}