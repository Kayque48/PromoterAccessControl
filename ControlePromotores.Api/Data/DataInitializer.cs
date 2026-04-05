using ControlePromotores.Api.BD;
using ControlePromotores.Api.Models;
using BCrypt.Net;

namespace ControlePromotores.Api.Data
{
    public static class DataInitializer
    {
        public static async Task InitializeAsync(PromotoresContext context)
        {
            // Aplicar migrations
            await context.Database.EnsureCreatedAsync();

            // Se já existe usuário admin, não fazer nada
            if (context.Usuarios.Any(u => u.Login == "admin"))
                return;

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
        }
    }
}
