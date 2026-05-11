# 🗄️ Banco de Dados e Models

> Documentação das entidades, relacionamentos e configuração do banco.
> ORM: Entity Framework Core | Banco dev: SQLite | Banco prod: MySQL (Pomelo 9.0)

---

## Diagrama de Entidades

```
┌─────────────────┐        ┌──────────────────────┐        ┌──────────────────┐
│    Empresa       │        │   PromotorEmpresa     │        │    Promotor      │
├─────────────────┤        ├──────────────────────┤        ├──────────────────┤
│ Id (PK)         │◄───────│ EmpresaId (FK)        │───────►│ Id (PK)          │
│ Nome            │        │ PromotorId (FK)        │        │ CPF (UNIQUE)     │
│ Telefone        │        │ DiasPermitidos (byte)  │        │ Nome             │
│ Endereco        │        │ Ativo                  │        │ Telefone         │
│ CNPJ            │        │ CriadoEm               │        │ Email            │
│ Ativo           │        └──────────────────────┘        │ Tipo             │
│ CriadoEm        │                                         │ EmpresaExclusiva │
└─────────────────┘                                         │ Ativo            │
        │                                                    │ CriadoEm         │
        │                                                    └──────────────────┘
        │                                                             │
        │                  ┌──────────────────┐                      │
        └──────────────────│    Registro       │──────────────────────┘
                           ├──────────────────┤
                           │ Id (PK)          │         ┌──────────────┐
                           │ PromotorId (FK)  │         │   Usuario    │
                           │ EmpresaId (FK)   │         ├──────────────┤
                           │ Tipo (entrada/   │         │ Id (PK)      │
                           │       saida)     │    ┌───►│ Nome         │
                           │ DataHora         │    │    │ Login        │
                           │ PermanenciaMin   │    │    │ SenhaHash    │
                           │ RegistradoPor ───┼────┘    │ Perfil       │
                           │ Observacao       │         │ Ativo        │
                           └──────────────────┘         └──────────────┘

                                         ┌────────────────────┐
                           Promotor ─────│  PromotorDocumento  │
                                         ├────────────────────┤
                                         │ Id (PK)            │
                                         │ PromotorId (FK)    │
                                         │ TipoDocumento      │
                                         │ Arquivo (path)     │
                                         └────────────────────┘
```

---

## Models — Detalhes

### `Promotor.cs`
```csharp
public class Promotor
{
    public int Id { get; set; }
    public string CPF { get; set; }          // MaxLength(11), UNIQUE
    public string Nome { get; set; }          // MaxLength(100), Required
    public string Telefone { get; set; }      // MaxLength(20)
    public string Email { get; set; }         // MaxLength(150)
    public string Tipo { get; set; }          // "promotor" | "exclusivo"
    public int? EmpresaExclusivaId { get; set; }  // só para tipo "exclusivo"
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }

    // Navigation properties
    public ICollection<PromotorEmpresa> PromotorEmpresas { get; set; }
    public ICollection<Registro> Registros { get; set; }
    public ICollection<PromotorDocumento> PromotorDocumentos { get; set; }
}
```

### `Registro.cs`
```csharp
public class Registro
{
    public int Id { get; set; }
    public int PromotorId { get; set; }       // FK → Promotor
    public int EmpresaId { get; set; }         // FK → Empresa
    public string Tipo { get; set; }           // "entrada" | "saida"
    public DateTime DataHora { get; set; }
    public int? PermanenciaMin { get; set; }   // calculado no PUT saida
    public int RegistradoPor { get; set; }     // FK → Usuario
    public string Observacao { get; set; }
}
```

### `PromotorEmpresa.cs` — Tabela de junção N:N
```csharp
public class PromotorEmpresa
{
    public int Id { get; set; }
    public int PromotorId { get; set; }
    public int EmpresaId { get; set; }
    public byte DiasPermitidos { get; set; } = 62;  // bitmask Seg-Sex
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
}
```

### `Usuario.cs`
```csharp
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Login { get; set; }       // UNIQUE
    public string SenhaHash { get; set; }   // BCrypt hash
    public string Perfil { get; set; }      // "admin" | "usuario"
    public bool Ativo { get; set; }
    public DateTime AtualizadoEm { get; set; }
}
```

---

## Configuração do Banco — appsettings.json

```json
{
  "UseSqlite": false,
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Database=ControlePromotores;User Id=root;Password=;",
    "SqliteConnection": "Data Source=database.db"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-muito-longa-e-segura-aqui-min-32-caracteres",
    "Issuer": "ControlePromotoresAPI",
    "Audience": "ControlePromotoresClient",
    "ExpirationMinutes": 1440
  }
}
```

> ⚠️ **Nunca commitar** `appsettings.json` com credenciais reais. Use `appsettings.Development.json` (já no `.gitignore`).

---

## Migrations (EF Core)

```bash
# Dentro de ControlePromotores.Api/

# Criar migration
dotnet ef migrations add NomeDaMigration

# Aplicar ao banco
dotnet ef database update

# Reverter última migration
dotnet ef database update NomeMigrationAnterior

# Listar migrations
dotnet ef migrations list
```

> ℹ️ O projeto usa `DataInitializer.cs` para seed automático ao iniciar. Cria o usuário `admin`/`senha123` se não existir.

---

## Trocar SQLite → MySQL

1. Em `appsettings.json`, preencha `MySqlConnection` com seus dados
2. Em `Program.cs`, substitua:
```csharp
// De:
options.UseSqlite(sqliteConnection)

// Para:
options.UseMySql(
    builder.Configuration.GetConnectionString("MySqlConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySqlConnection"))
)
```
3. Execute `dotnet ef database update`

→ Ver próximos passos em [[✅ Próximos Passos]]
