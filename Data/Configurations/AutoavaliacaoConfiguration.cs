// Configurações do EF Core do Modelo da Autoavaliação 
using Microsoft.EntityFrameworkCore; 
using Microsoft.EntityFrameworkCore.Metadata.Builders; 
using PensaAntes.Api.Models; 
 
namespace PensaAntes.Api.Data.Configurations; 
 
public class AutoAvaliacaoConfiguration : IEntityTypeConfiguration<AutoAvaliacao> 
{ 
    public void Configure(EntityTypeBuilder<AutoAvaliacao> builder) 
    { 
        builder.HasKey(a => a.Id); 
 
        builder.Property(a => a.OQueFezBem).IsRequired().HasMaxLength(2000); 
        builder.Property(a => a.OQuePodeMelhorar).IsRequired().HasMaxLength(2000); 
 
        // Resposta 1:1 AutoAvaliacao 
        builder.HasOne(a => a.Resposta) 
            .WithOne(r => r.AutoAvaliacao) 
            .HasForeignKey<AutoAvaliacao>(a => a.RespostaId) 
            .OnDelete(DeleteBehavior.Cascade); 
 
        builder.HasIndex(a => a.RespostaId).IsUnique(); 
    } 
}