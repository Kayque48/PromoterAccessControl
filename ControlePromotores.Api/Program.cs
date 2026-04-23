using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ControlePromotores.Api.BD;
using ControlePromotores.Api.Services;
using ControlePromotores.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Adicionar DbContext
// For local development, using SQLite by default
// To enable MySQL: run 'dotnet restore' first, then set UseSqlite to false in appsettings
var sqliteConnection = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=promoter_control.db";

builder.Services.AddDbContext<PromotoresContext>(options =>
    options.UseSqlite(sqliteConnection));

// Adicionar serviços
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<PromotorService>();
builder.Services.AddScoped<RegistroAcessoService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<RelatorioService>();

// Adicionar autenticação JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? "sua-chave-secreta-muito-longa-e-segura-aqui-min-32-caracteres";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ControlePromotoresAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ControlePromotoresClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

// Adicionar controllers
builder.Services.AddControllers();

// Adicionar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Adicionar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ControlePromotores.Api",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Digite: Bearer {seu token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Inicializar banco de dados
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PromotoresContext>();
    await DataInitializer.InitializeAsync(context);
}

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

