using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.Models;
    
    namespace ControlePromotores.Api.BD
    {
        public class PromotoresContext : DbContext
        {
            public PromotoresContext(DbContextOptions<PromotoresContext> options) : base(options)
            {
            }
            public DbSet<Promotor> Promotores { get; set; }
            public DbSet<Empresa> Empresas { get; set; }
            public DbSet<RegistroAcesso> RegistrosAcesso { get; set; }
            public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Promotor>()
                .HasOne(p => p.Empresa)
                .WithMany(e => e.Promotores)
                .HasForeignKey(p => p.EmpresaId);

            modelBuilder.Entity<RegistroAcesso>()
                .HasOne(r => r.Promotor)
                .WithMany(p => p.Registros)
                .HasForeignKey(r => r.PromotorId);

            // Configurar precisão de datas (opcional)
            modelBuilder.Entity<RegistroAcesso>()
                .Property(r => r.Entrada)
                .HasColumnType("datetime(6)");

            modelBuilder.Entity<RegistroAcesso>()
                .Property(r => r.Saida)
                .HasColumnType("datetime(6)");
        }

    }
    }