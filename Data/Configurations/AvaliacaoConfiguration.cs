// Configurações do EF Core do Modelo de Avaliação
using Microsoft.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore.Metadata.Builders; 
using PensaAntes.Api.Models; 
 
namespace PensaAntes.Api.Data.Configurations; 
 
public class AvaliacaoConfiguration : IEntityTypeConfiguration<Avaliacao> 
{ 
    public void Configure(EntityTypeBuilder<Avaliacao> builder) 
    { 
        builder.HasKey(a => a.Id); 
 
        builder.Property(a => a.Feedback).IsRequired().HasMaxLength(4000); 
 
        // Regra 0..3 garantida pelo banco, uma constraint por critério 
        builder.ToTable(t => 
        { 
            t.HasCheckConstraint("CK_Avaliacoes_Identificacao", "\"Identificacao\" BETWEEN 0 AND 3"); 
            t.HasCheckConstraint("CK_Avaliacoes_UsoDosDados", "\"UsoDosDados\" BETWEEN 0 AND 3"); 
            t.HasCheckConstraint("CK_Avaliacoes_Hipotese", "\"Hipotese\" BETWEEN 0 AND 3"); 
            t.HasCheckConstraint("CK_Avaliacoes_Justificativa", "\"Justificativa\" BETWEEN 0 AND 3"); 
            t.HasCheckConstraint("CK_Avaliacoes_PensamentoCritico", "\"PensamentoCritico\" BETWEEN 0 AND 3"); 
        }); 
 
        // Resposta 1:1 Avaliacao (a FK fica em Avaliacao) 
        builder.HasOne(a => a.Resposta) 
            .WithOne(r => r.Avaliacao) 
            .HasForeignKey<Avaliacao>(a => a.RespostaId) 
            .OnDelete(DeleteBehavior.Cascade); 
 
        // Índice único: no máximo uma Avaliacao por Resposta 
        builder.HasIndex(a => a.RespostaId).IsUnique(); 
    } 
}