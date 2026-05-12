using ControlePromotores.Api.BD;
using ControlePromotores.Api.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace ControlePromotores.Api.Data
{
    public static class DataInitializer
    {
        /// <summary>
        /// Inicializa dados padrão do sistema (usuário admin).
        /// Em modo MySQL: Assume que o banco foi criado via PromoterAccessControlDB.sql.
        /// Em modo SQLite: Cria schema via EnsureCreatedAsync (desenvolvimento).
        /// </summary>
        public static async Task InitializeAsync(PromotoresContext context, bool useSqlite = false)
        {
            try
            {
                // Apenas em SQLite (desenvolvimento offline) criamos schema automaticamente
                if (useSqlite)
                {
                    await context.Database.EnsureCreatedAsync();
                    Console.WriteLine("✅ Schema SQLite criado/verificado");
                }
                else
                {
                    // Em MySQL, assumimos que PromoterAccessControlDB.sql foi executado manualmente
                    Console.WriteLine("✅ Usando schema MySQL oficial (PromoterAccessControlDB.sql)");
                }

                // Verificar se admin já existe
                if (await context.Usuarios.AnyAsync(u => u.Login == "admin"))
                {
                    Console.WriteLine("✅ Usuário admin já existe, pulando inicialização");
                    return;
                }

                // Criar usuário admin padrão
                var usuarioAdmin = new Usuario
                {
                    Nome = "Administrador",
                    Login = "admin",
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword("senha123"),
                    Telefone = "0000-0000",
                    Cargo = "Administrador",
                    Perfil = "admin",
                    Ativo = true,
                };

                context.Usuarios.Add(usuarioAdmin);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Usuário admin criado com sucesso");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Erro ao inicializar dados: {ex.Message}");
                throw;
            }
        }
    }
}
