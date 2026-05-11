# 🚀 Como Rodar o Projeto

> Passo a passo completo para rodar o PromoterAccessControl localmente.

---

## Pré-requisitos

- [ ] **.NET 10 SDK** instalado → `dotnet --version` deve mostrar `10.x.x`
- [ ] **Python 3** (para servir o frontend) ou Node.js com `npx`
- [ ] **Git** para clonar/atualizar o repositório

---

## Passo 1 — Clonar o repositório

```bash
git clone https://github.com/seu-usuario/PromoterAccessControl.git
cd PromoterAccessControl
```

---

## Passo 2 — Iniciar o Backend

```bash
# Entrar na pasta do backend
cd ControlePromotores.Api

# Restaurar pacotes NuGet
dotnet restore

# Rodar a aplicação
dotnet run
```

**Saída esperada:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7272
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5297
```

**Acessar Swagger:** `https://localhost:7272/swagger`

> ℹ️ O banco SQLite (`database.db`) é criado automaticamente na primeira execução.
> O seed cria o usuário `admin` / `senha123` automaticamente.

---

## Passo 3 — Iniciar o Frontend

Abra **outro terminal**:

```bash
# Voltar para a raiz do projeto
cd ..

# Servir frontend com Python
cd frontend
python3 -m http.server 8000
```

**Acessar:** `http://localhost:8000/Login.html`

---

## Credenciais de Teste

| Campo | Valor |
|---|---|
| Login | `admin` |
| Senha | `senha123` |

---

## Diagrama do que deve estar rodando

```
Terminal 1 (Backend):
  ┌────────────────────────────────────┐
  │  dotnet run                        │
  │  → https://localhost:7272 (HTTPS)  │
  │  → http://localhost:5297 (HTTP)    │
  │  → /swagger disponível             │
  └────────────────────────────────────┘

Terminal 2 (Frontend):
  ┌────────────────────────────────────┐
  │  python3 -m http.server 8000       │
  │  → http://localhost:8000           │
  └────────────────────────────────────┘

Navegador:
  → http://localhost:8000/Login.html
```

---

## Troubleshooting

### ❌ "CORS bloqueado" no navegador
- Verifique se o backend está rodando
- Certifique-se de usar `http://localhost:8000` e não `file://`
- O `Program.cs` já configura `AllowAll` — se o erro persiste, reinicie o backend

### ❌ "Token inválido / 401 Unauthorized"
- Faça logout e login novamente
- Verifique se a `Jwt:Key` no `appsettings.json` tem no mínimo 32 caracteres

### ❌ "Porta em uso"
```bash
# Mudar porta em launchSettings.json
"applicationUrl": "https://localhost:7273;http://localhost:5298"
```
E atualizar `js/api.js`:
```javascript
const API_BASE_URL = 'https://localhost:7273/api';
```

### ❌ "dotnet: command not found" (Linux Mint)
```bash
# Instalar .NET 10
sudo apt install dotnet-sdk-10.0

# ou via snap
sudo snap install dotnet-sdk --classic --channel=10.0
```

### ❌ Banco corrompido / dados ruins
```bash
# Deletar o banco SQLite e recriar
cd ControlePromotores.Api
rm database.db database.db-shm database.db-wal
dotnet run   # Recria automaticamente com seed
```

---

## Trocar para MySQL (produção)

Edite `appsettings.json`:
```json
{
  "UseSqlite": false,
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Database=ControlePromotores;User Id=root;Password=suasenha;"
  }
}
```

E em `Program.cs`, troque `UseSqlite` por `UseMySql`.

→ Detalhes em [[🗄️ Banco de Dados e Models]]
