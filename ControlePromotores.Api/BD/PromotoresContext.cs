using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.Models;

namespace ControlePromotores.Api.BD
{
    public class PromotoresContext : DbContext
    {
        public PromotoresContext(DbContextOptions<PromotoresContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Promotor> Promotores { get; set; }
        public DbSet<RegistroAcesso> RegistrosAcesso { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração de Empresa
            modelBuilder.Entity<Empresa>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<Empresa>()
                .HasIndex(e => e.CNPJ)
                .IsUnique();

            modelBuilder.Entity<Empresa>()
                .Property(e => e.DataCriacao)
                .HasColumnType("datetime(6)");

            // Configuração de Promotor
            modelBuilder.Entity<Promotor>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Promotor>()
                .HasIndex(p => p.CPF)
                .IsUnique();

            modelBuilder.Entity<Promotor>()
                .HasOne(p => p.Empresa)
                .WithMany(e => e.Promotores)
                .HasForeignKey(p => p.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Promotor>()
                .Property(p => p.DataContratacao)
                .HasColumnType("datetime(6)");

            // Configuração de RegistroAcesso
            modelBuilder.Entity<RegistroAcesso>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<RegistroAcesso>()
                .HasOne(r => r.Promotor)
                .WithMany(p => p.Registros)
                .HasForeignKey(r => r.PromotorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RegistroAcesso>()
                .Property(r => r.Entrada)
                .HasColumnType("datetime(6)");

            modelBuilder.Entity<RegistroAcesso>()
                .Property(r => r.Saida)
                .HasColumnType("datetime(6)");

            // Configuração de Usuario
            modelBuilder.Entity<Usuario>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Login)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.DataCriacao)
                .HasColumnType("datetime(6)");

            modelBuilder.Entity<Usuario>()
                .Property(u => u.UltimoLogin)
                .HasColumnType("datetime(6)");
        }
    }
}
