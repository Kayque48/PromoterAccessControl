using Microsoft.EntityFrameworkCore;
using ControlePromotores.Api.Models;

namespace ControlePromotores.Api.BD
{
    /// <summary>
    /// Contexto de dados responsável pela persistência de todas as entidades do sistema de controle de promotores.
    /// Implementa o padrão Repository através de Entity Framework Core para abstrair operações CRUD e garantir
    /// separação de responsabilidades entre a camada de dados e a lógica de negócio.
    /// </summary>
    public class PromotoresContext : DbContext
    {
        public PromotoresContext(DbContextOptions<PromotoresContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Promotor> Promotores { get; set; }
        public DbSet<Registro> Registros { get; set; }
        public DbSet<PromotorEmpresa> PromotoresEmpresas { get; set; }
        public DbSet<PromotorDocumento> PromotoresDocumentos { get; set; }

        /// <summary>
        /// Configura o mapeamento object-relational (ORM) das entidades para a estrutura do banco de dados,
        /// incluindo constraints, índices para otimização de queries, e relacionamentos com políticas de deleção.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // CONFIGURAÇÃO: EMPRESA
            // ==========================================
            modelBuilder.Entity<Empresa>(entity =>
            {
                entity.ToTable("empresas");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.CNPJ)
                    .IsRequired()
                    .HasMaxLength(14)
                    .HasColumnName("cnpj");

                entity.HasIndex(e => e.CNPJ)
                    .IsUnique()
                    .HasDatabaseName("uq_empresas_cnpj");

                // Restrição de unicidade: CNPJ é identificador único de empresa no Brasil.
                // Garante integridade referencial e consultas O(1) por CNPJ.
                entity.Property(e => e.RazaoSocial)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("razao_social");

                entity.Property(e => e.NomeFantasia)
                    .HasMaxLength(150)
                    .HasColumnName("nome_fantasia");

                entity.Property(e => e.Telefone)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("telefone");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("email");

                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("uq_empresas_email");

                entity.Property(e => e.Endereco)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("endereco");

                entity.Property(e => e.Ativo)
                    .HasDefaultValue(true)
                    .HasColumnName("ativo");

                entity.Property(e => e.CriadoEm)
                    .HasColumnType("TEXT")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnName("criado_em");

                entity.Property(e => e.AtualizadoEm)
                    .HasColumnType("TEXT")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnName("atualizado_em");

                // Tipo TEXT + DEFAULT CURRENT_TIMESTAMP: Compatibilidade SQLite (desenvolvimento) e MySQL/MariaDB (produção).
                // Auditoria: Registra automaticamente timestamps de criação e última atualização via banco de dados.

                // Relacionamentos
                // Política DeleteBehavior.Restrict: Impede deleção de empresa se houver promotores exclusivos vinculados.
                // Garante integridade referencial e preserva auditoria de promotores (histórico não é perdido).
                entity.HasMany(e => e.PromotoresExclusivos)
                    .WithOne(p => p.EmpresaExclusiva)
                    .HasForeignKey(p => p.EmpresaExclusivaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.PromotorEmpresas)
                    .WithOne(pe => pe.Empresa)
                    .HasForeignKey(pe => pe.EmpresaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Registros)
                    .WithOne(r => r.Empresa)
                    .HasForeignKey(r => r.EmpresaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.PromotorDocumentos)
                    .WithOne(pd => pd.Empresa)
                    .HasForeignKey(pd => pd.EmpresaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // CONFIGURAÇÃO: USUARIO
            // ==========================================
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(u => u.Nome)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("nome");

                entity.Property(u => u.Login)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("login");

                entity.HasIndex(u => u.Login)
                    .IsUnique()
                    .HasDatabaseName("uq_usuarios_login");

                // Restrição de unicidade: Login é identificador único para autenticação.
                // Garante que cada usuário tem credencial exclusiva no sistema.

                entity.Property(u => u.SenhaHash)
                    .IsRequired()
                    .HasColumnName("senha_hash");

                entity.Property(u => u.Telefone)
                    .HasMaxLength(20)
                    .HasColumnName("telefone");

                entity.Property(u => u.Cargo)
                    .HasMaxLength(80)
                    .HasColumnName("cargo");

                entity.Property(u => u.Perfil)
                    .IsRequired()
                    .HasColumnName("perfil");

                entity.Property(u => u.Ativo)
                    .HasDefaultValue(true)
                    .HasColumnName("ativo");

                entity.Property(u => u.CriadoEm)
                    .HasColumnType("TEXT")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnName("criado_em");

                entity.Property(u => u.AtualizadoEm)
                    .HasColumnType("TEXT")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnName("atualizado_em");

                // Relacionamentos
                // Rastreabilidade: RegistradoPor referencia o usuário (admin/sistema) que registrou a entrada/saída.
                // Crítico para auditoria e identificação de responsável por cada operação.
                entity.HasMany(u => u.RegistrosRegistrados)
                    .WithOne(r => r.UsuarioRegistrador)
                    .HasForeignKey(r => r.RegistradoPor)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // CONFIGURAÇÃO: PROMOTOR
            // ==========================================
            modelBuilder.Entity<Promotor>(entity =>
            {
                entity.ToTable("promotores");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(p => p.CPF)
                    .IsRequired()
                    .HasMaxLength(11)
                    .HasColumnName("cpf");

                entity.HasIndex(p => p.CPF)
                    .IsUnique()
                    .HasDatabaseName("uq_promotores_cpf");

                // Restrição de unicidade: CPF é identificador único de cidadão brasileiro.
                // Impede registro duplicado de mesmo promotor no sistema.

                entity.Property(p => p.Nome)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("nome");

                entity.Property(p => p.Telefone)
                    .HasMaxLength(20)
                    .HasColumnName("telefone");

                entity.Property(p => p.Email)
                    .HasMaxLength(150)
                    .HasColumnName("email");

                entity.Property(p => p.Tipo)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("promotor")
                    .HasColumnName("tipo");

                // Tipo de promotor: define modelo de alocação.
                // - "promotor": Pode trabalhar para múltiplas empresas (N:N via PromotorEmpresa).
                // - "exclusivo": Vinculado a uma única empresa (EmpresaExclusivaId).
                // Decisão arquitetural: Suporta dois modelos de negócio simultâneos no mesmo banco.

                entity.Property(p => p.EmpresaExclusivaId)
                    .HasColumnName("empresa_exclusiva_id");

                entity.Property(p => p.Ativo)
                    .HasDefaultValue(true)
                    .HasColumnName("ativo");

                entity.Property(p => p.CriadoEm)
                    .HasColumnType("TEXT")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnName("criado_em");

                entity.Property(p => p.AtualizadoEm)
                    .HasColumnType("TEXT")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnName("atualizado_em");

                // FK para Empresa Exclusiva (opcional)
                entity.HasOne(p => p.EmpresaExclusiva)
                    .WithMany(e => e.PromotoresExclusivos)
                    .HasForeignKey(p => p.EmpresaExclusivaId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento N:N via PromotorEmpresa
                entity.HasMany(p => p.PromotorEmpresas)
                    .WithOne(pe => pe.Promotor)
                    .HasForeignKey(pe => pe.PromotorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento com Registros
                entity.HasMany(p => p.Registros)
                    .WithOne(r => r.Promotor)
                    .HasForeignKey(r => r.PromotorId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relacionamento com Documentos
                entity.HasMany(p => p.PromotorDocumentos)
                    .WithOne(pd => pd.Promotor)
                    .HasForeignKey(pd => pd.PromotorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // CONFIGURAÇÃO: PROMOTOR_EMPRESA (N:N)
            // ==========================================
            // Tabela de junção que implementa relacionamento muitos-para-muitos.
            // Permite que um promotor (genérico) trabalhe em múltiplas empresas,
            // com controle granular de dias permitidos por alocação (DiasPermitidos).
            modelBuilder.Entity<PromotorEmpresa>(entity =>
            {
                entity.ToTable("promotor_empresa");
                entity.HasKey(pe => pe.Id);

                entity.Property(pe => pe.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(pe => pe.PromotorId)
                    .IsRequired()
                    .HasColumnName("promotor_id");

                entity.Property(pe => pe.EmpresaId)
                    .IsRequired()
                    .HasColumnName("empresa_id");

                // UNIQUE constraint: (PromotorId, EmpresaId)
                entity.HasIndex(pe => new { pe.PromotorId, pe.EmpresaId })
                    .IsUnique()
                    .HasDatabaseName("uq_promotor_empresa");

                entity.Property(pe => pe.DiasPermitidos)
                    .HasDefaultValue((byte)62)
                    .HasColumnName("dias_permitidos");

                entity.Property(pe => pe.Ativo)
                    .HasDefaultValue(true)
                    .HasColumnName("ativo");

                entity.Property(pe => pe.CriadoEm)
                    .HasColumnType("TEXT")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnName("criado_em");

                // Relacionamentos
                entity.HasOne(pe => pe.Promotor)
                    .WithMany(p => p.PromotorEmpresas)
                    .HasForeignKey(pe => pe.PromotorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(pe => pe.Empresa)
                    .WithMany(e => e.PromotorEmpresas)
                    .HasForeignKey(pe => pe.EmpresaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // CONFIGURAÇÃO: REGISTRO (Entrada/Saída)
            // ==========================================
            modelBuilder.Entity<Registro>(entity =>
            {
                entity.ToTable("registros");
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(r => r.PromotorId)
                    .IsRequired()
                    .HasColumnName("promotor_id");

                entity.Property(r => r.EmpresaId)
                    .IsRequired()
                    .HasColumnName("empresa_id");

                entity.Property(r => r.Tipo)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("tipo");

                entity.Property(r => r.DataHora)
                    .IsRequired()
                    .HasColumnType("TEXT")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnName("data_hora");

                entity.Property(r => r.PermanenciaMin)
                    .HasColumnName("permanencia_min");

                entity.Property(r => r.RegistradoPor)
                    .IsRequired()
                    .HasColumnName("registrado_por");

                entity.Property(r => r.Observacao)
                    .HasMaxLength(255)
                    .HasColumnName("observacao");

                // Índices para performance
                // Otimizam queries de relatórios e filtragens por promotor, empresa e período.
                // Crítico: Tabela Registros cresce continuamente (append-only para auditoria).
                entity.HasIndex(r => r.PromotorId)
                    .HasDatabaseName("idx_reg_promotor");

                entity.HasIndex(r => r.EmpresaId)
                    .HasDatabaseName("idx_reg_empresa");

                entity.HasIndex(r => r.DataHora)
                    .HasDatabaseName("idx_reg_data_hora");

                // Relacionamentos
                entity.HasOne(r => r.Promotor)
                    .WithMany(p => p.Registros)
                    .HasForeignKey(r => r.PromotorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Empresa)
                    .WithMany(e => e.Registros)
                    .HasForeignKey(r => r.EmpresaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.UsuarioRegistrador)
                    .WithMany(u => u.RegistrosRegistrados)
                    .HasForeignKey(r => r.RegistradoPor)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // CONFIGURAÇÃO: PROMOTOR_DOCUMENTO
            // ==========================================
            modelBuilder.Entity<PromotorDocumento>(entity =>
            {
                entity.ToTable("promotor_documentos");
                entity.HasKey(pd => pd.Id);

                entity.Property(pd => pd.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(pd => pd.PromotorId)
                    .IsRequired()
                    .HasColumnName("promotor_id");

                entity.Property(pd => pd.EmpresaId)
                    .IsRequired()
                    .HasColumnName("empresa_id");

                entity.Property(pd => pd.TipoDocumento)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnName("tipo_doc");

                entity.Property(pd => pd.Caminho)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("caminho");

                entity.Property(pd => pd.EnviadoEm)
                    .HasColumnType("TEXT")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnName("enviado_em");

                // Relacionamentos
                entity.HasOne(pd => pd.Promotor)
                    .WithMany(p => p.PromotorDocumentos)
                    .HasForeignKey(pd => pd.PromotorId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pd => pd.Empresa)
                    .WithMany(e => e.PromotorDocumentos)
                    .HasForeignKey(pd => pd.EmpresaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

