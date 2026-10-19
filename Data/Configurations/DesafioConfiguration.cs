// Configurações do EF Core do Modelo do Desafio 
using Microsoft.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore.Metadata.Builders; 
using PensaAntes.Api.Models; 
 
namespace PensaAntes.Api.Data.Configurations; 
 
public class DesafioConfiguration : IEntityTypeConfiguration<Desafio> 
{ 
    public void Configure(EntityTypeBuilder<Desafio> builder) 
    { 
        builder.HasKey(d => d.Id); 
 
        builder.Property(d => d.Titulo).IsRequired().HasMaxLength(150); 
        builder.Property(d => d.Descricao).IsRequired().HasMaxLength(1000); 
        builder.Property(d => d.SituacaoProblema).IsRequired().HasMaxLength(4000); 
        builder.Property(d => d.DadosDisponiveis).IsRequired().HasMaxLength(4000); 
        builder.Property(d => d.Objetivo).IsRequired().HasMaxLength(1000); 
        builder.Property(d => d.PerguntaPrincipal).IsRequired().HasMaxLength(500); 
 
        // Enum criado como numero será salvo como texto ("Facil", "Medio", "Dificil") 
        builder.Property(d => d.Dificuldade) 
            .IsRequired() 
            .HasConversion<string>() 
            .HasMaxLength(20); 
 
        // Categoria 1:N Desafio (FK está aqui, então configuramos aqui) 
        builder.HasOne(d => d.Categoria) 
            .WithMany(c => c.Desafios) 
            .HasForeignKey(d => d.CategoriaId) 
            .OnDelete(DeleteBehavior.Restrict); 
    } 
}