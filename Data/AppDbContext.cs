// Configurações do Banco de Dados do Modelo 
using Microsoft.EntityFrameworkCore; 
using PensaAntes.Api.Models; 
 
namespace PensaAntes.Api.Data; 
 
public class AppDbContext : DbContext 
{ 
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } 
 
    public DbSet<Usuario> Usuarios => Set<Usuario>(); 
    public DbSet<Categoria> Categorias => Set<Categoria>(); 
    public DbSet<Desafio> Desafios => Set<Desafio>(); 
    public DbSet<Resposta> Respostas => Set<Resposta>(); 
    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>(); 
    public DbSet<AutoAvaliacao> AutoAvaliacoes => Set<AutoAvaliacao>(); 
 
    protected override void OnModelCreating(ModelBuilder modelBuilder) 
    { 
        // Encontra e aplica automaticamente todas as classes IEntityTypeConfiguration<T> definida na pasta Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly); 
    } 
}