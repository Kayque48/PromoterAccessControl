# 📖 Guia Rápido — Onde Está Cada Coisa

> Arquivo de navegação rápida. Cada item leva direto ao arquivo ou pasta do projeto.
> Sem explicações longas — só o essencial para você se mover rápido.

---

## 🔧 Backend (API)

**`ControlePromotores.Api/`** — Aplicação ASP.NET Core. Toda a lógica de negócio, autenticação e acesso ao banco ficam aqui.
→ [[📡 Controllers e Endpoints]]

**`Controllers/`** — Recebem as requisições HTTP e devolvem respostas JSON.
→ `AuthController`, `PromotoresController`, `EmpresasController`, `RegistrosAcessoController`, `DashboardController`, `RelatoriosController`

**`Services/`** — Lógica de negócio separada dos controllers.
→ `TokenService`, `EmpresaService`, `PromotorService`, `RegistroAcessoService`, `DashboardService`, `RelatorioService`

**`Models/`** — Entidades do banco de dados (tabelas).
→ [[🗄️ Banco de Dados e Models]]

**`DTOs/`** — Objetos de transferência de dados (o que entra e sai da API).
→ `LoginRequest`, `PromotorDto`, `EmpresaDto`, `RegistroAcessoDto`, `DashboardDto`, `RelatorioDto`

**`BD/PromotoresContext.cs`** — Configuração do Entity Framework Core (relacionamentos, tabelas).
→ [[🗄️ Banco de Dados e Models]]

**`Program.cs`** — Ponto de entrada. Configura JWT, CORS, Swagger, banco de dados.
→ [[🗺️ Visão Geral]]

**`appsettings.json`** — Configuração de conexão (SQLite/MySQL) e chave JWT.

---

## 🖥️ Frontend

**`frontend/`** — Aplicação web em HTML/CSS/JS puro. Sem framework.
→ [[🖥️ Frontend — Telas e Arquivos]]

**`frontend/js/api.js`** — Todas as chamadas HTTP para o backend. Ponto central de comunicação.

**`frontend/js/auth.js`** — Gerencia token JWT no `localStorage`. Login/logout.

**`frontend/Login.html`** — Primeira tela. Formulário de autenticação.

**`frontend/Dashboard.html`** — Visão geral com dados do dia.

**`frontend/Promotores.html`** — CRUD completo de promotores.

**`frontend/Empresas.html`** — CRUD completo de empresas.

**`frontend/Registro-ponto.html`** — Registrar entrada e saída de promotores.

**`frontend/Relatorios.html`** — Filtros e visualização de registros.

**`frontend/Exportar.html`** — Download dos dados em CSV.

---

## 📋 Documentação

**`README.md`** — Visão geral do projeto em português.

**`README_EN.md`** — Visão geral em inglês.

**`GUIA_EXECUCAO.md`** — Como rodar o projeto passo a passo.
→ [[🚀 Como Rodar o Projeto]]

---

## 🗺️ Mapa Visual da Estrutura

```
PromoterAccessControl/
│
├── ControlePromotores.Api/        ← BACKEND
│   ├── Controllers/               ← Endpoints HTTP
│   ├── Services/                  ← Lógica de negócio
│   ├── Models/                    ← Entidades do banco
│   ├── DTOs/                      ← Entrada/saída da API
│   ├── BD/                        ← DbContext (EF Core)
│   ├── Data/                      ← Seed inicial do banco
│   ├── Program.cs                 ← Configuração geral
│   └── appsettings.json           ← Strings de conexão / JWT
│
└── frontend/                      ← FRONTEND
    ├── *.html                     ← Telas
    ├── js/                        ← Lógica JavaScript
    ├── css/                       ← Estilos
    └── img/                       ← Imagens
```
