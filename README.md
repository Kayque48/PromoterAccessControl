# ControlePromotores.Api

API REST desenvolvida em **ASP.NET Core (.NET 10)** para controle de acesso de promotores em empresas. Permite registrar entrada e saída de promotores, gerenciar empresas e promotores, e autenticar usuários via **JWT**.

---

## Objetivo

O ControlePromotores.Api tem como objetivo fornecer uma solução centralizada para o controle de acesso e monitoramento de promotores vinculados a empresas. O sistema permite registrar e validar o cadastro de empresas e promotores, controlar as movimentações de entrada e saída com rastreabilidade completa, e garantir que apenas promotores autorizados realizem registros nos dias permitidos.
Por meio de autenticação segura com JWT e controle de perfis de acesso, o sistema assegura que cada usuário opere apenas dentro das funcionalidades permitidas ao seu nível de permissão. Além disso, disponibiliza um dashboard com informações consolidadas sobre a presença e o histórico de movimentações dos promotores, oferecendo uma visão gerencial em tempo real para os administradores.
A solução foi desenvolvida com foco em integridade dos dados, segurança das informações e escalabilidade, utilizando tecnologias modernas como ASP.NET Core, Entity Framework Core e MySQL, sendo capaz de atender empresas de diferentes portes que necessitem de controle eficiente sobre a atuação de seus promotores em campo.


## Tecnologias

| Pacote | Versão |
|--------|--------|
| .NET | 10.0 |
| Entity Framework Core | 10.0.3 |
| Pomelo EF Core MySQL | 9.0.0 |
| ASP.NET Authentication JwtBearer | 10.0.3 |
| System.IdentityModel.Tokens.Jwt | 8.16.0 |
| BCrypt.Net-Next | 4.1.0 |

---

## Estrutura do Projeto

```
ControlePromotores.Api/
├── BD/
│   └── PromotoresContext.cs       # DbContext e configuração dos relacionamentos
├── Controllers/
│   ├── ControladorAuticacao.cs    # Autenticação e geração de token JWT
│   ├── ControladorPromotores.cs   # CRUD de promotores
│   └── ControladorRegistros.cs    # Registro de entrada e saída
├── Models/
│   ├── Empresa.cs                 # Entidade Empresa
│   ├── Promotor.cs                # Entidade Promotor
│   ├── RegistroAcesso.cs          # Entidade de registro de ponto
│   ├── Usuario.cs                 # Entidade de usuário do sistema
│   └── LoginModel.cs              # DTO de login
├── Services/
│   └── TokenService.cs            # Geração de token JWT
├── appsettings.json
└── Program.cs
```

---

## Entidades e Relacionamentos

```
Empresa (1) ──── (N) Promotor (1) ──── (N) RegistroAcesso
```

- Uma **Empresa** possui vários **Promotores**
- Um **Promotor** possui vários **RegistrosAcesso**
- **Usuario** é independente — representa quem opera o sistema

---

## Endpoints

### Autenticação

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/auth/login` | Realiza login e retorna o token JWT |

**Body:**
```json
{
  "login": "admin",
  "senha": "senha123"
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

---

### Promotores `🔒 Requer autenticação`

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/api/promotores` | Lista todos os promotores |
| `GET` | `/api/promotores?empresaId=1` | Lista promotores filtrados por empresa |
| `POST` | `/api/promotores` | Cria um novo promotor |

---

### Registros de Acesso `🔒 Requer autenticação`

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/registros/entrada` | Registra entrada de um promotor |
| `PUT` | `/api/registros/saida/{id}` | Registra saída e calcula tempo de permanência |
| `GET` | `/api/registros/promotor/{promotorId}` | Lista registros de um promotor |

---

## Configuração

### appsettings.json

Adicione a string de conexão com o banco e o segredo JWT:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=controle_promotores;User=root;Password=suasenha;"
  },
  "Jwt": {
    "Secret": "sua_chave_secreta_aqui_minimo_32_caracteres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

### Program.cs

O `Program.cs` precisa ser configurado com os serviços necessários:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<TokenService>();

// Entity Framework + MySQL
builder.Services.AddDbContext<PromotoresContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

// JWT
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Secret"]);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## Como Rodar

**1. Clone o repositório**
```bash
git clone https://github.com/seu-usuario/ControlePromotores.Api.git
cd ControlePromotores.Api
```

**2. Configure o banco de dados** no `appsettings.json` conforme mostrado acima.

**3. Execute as migrations**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

**4. Rode a aplicação**
```bash
dotnet run
```

A API estará disponível em:
- HTTP: `http://localhost:5297`
- HTTPS: `https://localhost:7272`

---

## Problemas Conhecidos

> O projeto ainda contém bugs que impedem a compilação. Antes de rodar, corrija os seguintes pontos:

- `[httpGet]` → `[HttpGet]` em `ControladorPromotores.cs`
- Bloco de código solto fora de método em `ControladorAuticacao.cs`
- `[required]` → `[Required]` em `LoginModel.cs`
- `System.componentModel` → `System.ComponentModel` em `Promotor.cs`
- `AppDbContext` → `PromotoresContext` em `ControladorRegistros.cs`
- Query com `.include` e `.Asqueryable()` fora de contexto em `ControladorPromotores.cs`
- `Program.cs` não registra nenhum serviço — ver seção de configuração acima

---

## Licença

Este projeto está sob a licença MIT.
