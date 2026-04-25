// Importa o Entity Framework Core, que é a biblioteca responsável
// por fazer a comunicação entre o C# e o banco de dados
using Microsoft.EntityFrameworkCore;

// Importa os Models (modelos) do projeto, que representam as tabelas do banco
using PromoterAccessControl.Models;

namespace PromoterAccessControl.Data
{
    // AppDbContext é a classe que representa o banco de dados dentro do código C#
    // Ela herda de DbContext, que é a classe base do Entity Framework Core
    // Pense nela como uma "ponte" entre o código e o banco de dados MySQL
    public class AppDbContext : DbContext
    {
        // Este é o construtor da classe — ele é chamado automaticamente quando
        // a aplicação inicializa e precisa se conectar ao banco de dados
        // O parâmetro "options" carrega as configurações da conexão (ex: endereço do servidor,
        // nome do banco, usuário e senha), que são definidas no appsettings.json
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet representa uma tabela do banco de dados
        // "Companies" será o nome da tabela no MySQL
        // O tipo <Company> indica que cada linha da tabela segue o formato do Model Company
        public DbSet<Company> Companies { get; set; }

        // Tabela de promotores — cada registro seguirá o formato do Model Promoter
        public DbSet<Promoter> Promoters { get; set; }

        // Tabela de logs de acesso — registra entradas e saídas dos promotores
        public DbSet<AccessLog> AccessLogs { get; set; }
    }
}