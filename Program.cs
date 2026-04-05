using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.Models;
using ControlePromotores.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

builder.Services.AddDbContext<PromotoresContext>(options =>
{
    options.UseInMemoryDatabase("ControlePromotoresDb");
});

builder.Services.AddScoped<TokenService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"] ?? "MinhaChaveSecretaMuitoSegura123!"))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PromotoresContext>();
    if (!db.Empresas.Any())
    {
        var empresa1 = new Empresa { Cnpj = "12.345.678/0001-90", RazaoSocial = "Empresa ABC Ltda", NomeFantasia = "ABC", Telefone = "(11) 1111-1111", EmailCorporativo = "contato@abc.com", Endereco = "Rua A, 100" };
        var empresa2 = new Empresa { Cnpj = "98.765.432/0001-10", RazaoSocial = "Empresa XYZ S.A.", NomeFantasia = "XYZ", Telefone = "(11) 2222-2222", EmailCorporativo = "contato@xyz.com", Endereco = "Av. B, 200" };
        db.Empresas.AddRange(empresa1, empresa2);

        db.Promotores.AddRange(
            new Promotor { Nome = "João Silva", Cpf = "123.456.789-00", Telefone = "(11) 91234-5678", Email = "joao@abc.com", Empresa = empresa1, Ativo = true, Categoria = "Promotor", DiasPermitidos = "Monday,Tuesday,Wednesday,Thursday,Friday" },
            new Promotor { Nome = "Maria Souza", Cpf = "987.654.321-00", Telefone = "(11) 98765-4321", Email = "maria@xyz.com", Empresa = empresa2, Ativo = true, Categoria = "Promotor Exclusivo", DiasPermitidos = "Monday,Tuesday,Wednesday,Thursday,Friday" }
        );

        var senha = BCrypt.Net.BCrypt.HashPassword("senha123");
        db.Usuarios.Add(new Usuario { Nome = "Administrador", Login = "admin", SenhaHash = senha, Perfil = "admin" });

        db.SaveChanges();
    }
}

app.Run();
