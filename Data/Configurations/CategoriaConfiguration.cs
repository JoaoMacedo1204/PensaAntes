// Configurações do EF Core para os Modelos da Categoria

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensaAntes.Api.Models;

namespace PensaAntes.Api.Data.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Descricao)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(c => c.Nome)
            .IsUnique();
    }
}