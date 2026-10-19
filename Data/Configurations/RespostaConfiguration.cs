// Configurações do EF Core do Modelo de Respostas do Desafio
using Microsoft.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore.Metadata.Builders; 
using PensaAntes.Api.Models; 
 
namespace PensaAntes.Api.Data.Configurations; 
 
public class RespostaConfiguration : IEntityTypeConfiguration<Resposta> 
{ 
    public void Configure(EntityTypeBuilder<Resposta> builder) 
    { 
        builder.HasKey(r => r.Id); 
 
        // Etapa 1: obrigatória 
        builder.Property(r => r.AnaliseInicial).IsRequired().HasMaxLength(5000); 
        builder.Property(r => r.JustificativaInicial).IsRequired().HasMaxLength(5000); 
 
        // Etapa 2: opcional. "text" = sem limite, pois a resposta da IA pode ser longa 
        builder.Property(r => r.AnaliseIA).HasColumnType("text"); 
 
        // Etapa 3: opcional até a finalização 
        builder.Property(r => r.DecisaoFinal) 
            .HasConversion<string>() 
            .HasMaxLength(30); 
        builder.Property(r => r.JustificativaFinal).HasMaxLength(5000); 
 
        builder.Property(r => r.DataInicio).IsRequired(); 
 
        // Se o Usuário optar pela análise da IA, ConsultouIA será true 
        builder.ToTable(t => t.HasCheckConstraint( 
            "CK_Respostas_AnaliseIA_ConsultouIA", 
            "\"AnaliseIA\" IS NULL OR \"ConsultouIA\" = TRUE")); 
 
        // Usuario 1:N Resposta 
        builder.HasOne(r => r.Usuario) 
            .WithMany(u => u.Respostas) 
            .HasForeignKey(r => r.UsuarioId) 
            .OnDelete(DeleteBehavior.Restrict); 
 
        // Desafio 1:N Resposta 
        builder.HasOne(r => r.Desafio) 
            .WithMany(d => d.Respostas) 
            .HasForeignKey(r => r.DesafioId) 
            .OnDelete(DeleteBehavior.Restrict); 
    } 
}