# 🗺️ Visão Geral — Arquitetura do Projeto

## O que é o PromoterAccessControl?

Sistema de **controle de acesso de promotores** vinculados a empresas. Registra entrada/saída, calcula tempo de permanência, gera relatórios e exporta dados em CSV. Autenticação por JWT com perfis de acesso (`admin` / `usuario`).

---

## Diagrama de Arquitetura

```
┌─────────────────────────────────────────────────┐
│                   FRONTEND                       │
│         HTML + CSS + JavaScript Puro             │
│                                                  │
│  Login → Dashboard → Promotores → Empresas       │
│       → Registro de Ponto → Relatórios → Exportar│
│                                                  │
│  js/api.js ──── fetch() com Bearer Token ───────┤
└─────────────────────┬───────────────────────────┘
                       │ HTTPS
                       ▼
┌─────────────────────────────────────────────────┐
│              ASP.NET CORE API (.NET 10)          │
│                                                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────────┐   │
│  │  Auth    │  │ Promotores│  │   Empresas   │   │
│  │Controller│  │Controller│  │  Controller  │   │
│  └────┬─────┘  └────┬─────┘  └──────┬───────┘   │
│       │              │               │            │
│  ┌────▼──────────────▼───────────────▼────────┐  │
│  │              Services Layer                 │  │
│  │  TokenService | PromotorService             │  │
│  │  EmpresaService | RegistroAcessoService     │  │
│  │  DashboardService | RelatorioService        │  │
│  └────────────────────┬────────────────────────┘  │
│                        │                           │
│  ┌─────────────────────▼──────────────────────┐   │
│  │         Entity Framework Core              │   │
│  │         PromotoresContext (DbContext)       │   │
│  └─────────────────────┬──────────────────────┘   │
└────────────────────────┼───────────────────────────┘
                          │
                          ▼
              ┌───────────────────┐
              │     BANCO DE DADOS │
              │  SQLite (dev)      │
              │  MySQL  (prod)     │
              └───────────────────┘
```

---

## Fluxo de Autenticação

```
Usuário          Frontend             API                  Banco
   │                │                  │                     │
   │──── login ────►│                  │                     │
   │                │──POST /api/auth/login ──────────────► │
   │                │                  │◄── busca usuário ── │
   │                │                  │── BCrypt.Verify() ──┤
   │                │◄── { token } ────│                     │
   │                │── salva no ──────│                     │
   │                │   localStorage   │                     │
   │                │                  │                     │
   │── acessa tela ►│                  │                     │
   │                │──GET /api/... ───►                     │
   │                │  Authorization:  │                     │
   │                │  Bearer {token}  │                     │
   │                │◄── dados JSON ───│                     │
```

---

## Tecnologias

| Camada | Tecnologia | Versão |
|---|---|---|
| Backend | ASP.NET Core | .NET 10 |
| ORM | Entity Framework Core | 10.x |
| Banco (dev) | SQLite | — |
| Banco (prod) | MySQL via Pomelo | 9.0.0 |
| Autenticação | JWT Bearer | 10.0.3 |
| Hash de senha | BCrypt.Net-Next | 4.1.0 |
| Documentação | Swagger / OpenAPI | — |
| Frontend | HTML + CSS + JS | Puro |

---

## Relacionamentos do Banco

```
Empresa (1) ──────────────────── (N) PromotorEmpresa (N) ──── (1) Promotor
                                         │ DiasPermitidos (bitmask)
                                         │ Ativo

Promotor (1) ──── (N) Registro
                       │ Tipo: "entrada" | "saida"
                       │ PermanenciaMin (calculado)
                       │ RegistradoPor → Usuario

Usuario (independente)
   │ Perfil: "admin" | "usuario"
   │ SenhaHash (BCrypt)
   └── opera o sistema
```

---

## Program.cs — O que está configurado

```csharp
// ✅ DbContext com SQLite (troca para MySQL via appsettings)
builder.Services.AddDbContext<PromotoresContext>(...);

// ✅ Todos os Services registrados
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<EmpresaService>();
builder.Services.AddScoped<PromotorService>();
builder.Services.AddScoped<RegistroAcessoService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<RelatorioService>();

// ✅ JWT com validação de issuer + audience
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)...

// ✅ CORS liberado (AllowAll — restringir em produção)
builder.Services.AddCors(...);

// ✅ Swagger com suporte a Bearer token
builder.Services.AddSwaggerGen(...);

// ✅ Seed automático do banco ao iniciar
await DataInitializer.InitializeAsync(context);
```

→ Ver detalhes de cada módulo em [[📡 Controllers e Endpoints]] e [[🗄️ Banco de Dados e Models]]
