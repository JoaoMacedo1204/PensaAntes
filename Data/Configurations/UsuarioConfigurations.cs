// Configurações do EF Core para o Models do Usuário

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PensaAntes.Api.Models;

namespace PensaAntes.Api.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(u => u.AreaPreferencia)
            .HasMaxLength(100);

        // Dois usuários não podem ter o mesmo e-mail
        builder.HasIndex(u => u.Email)
            .IsUnique();
    }
}