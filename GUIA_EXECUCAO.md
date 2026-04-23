# 🚀 Guia de Execução - PromoterAccessControl

## Pré-requisitos

- **.NET 10** instalado
- **Node.js** (opcional, se usar npm para servir frontend)
- **SQLite** (padrão) ou **MySQL** configurado

---

## 1️⃣ Iniciar o Backend

### No terminal, navegue até `ControlePromotores.Api`:

```bash
cd ControlePromotores.Api
dotnet restore
dotnet run
```

O backend estará disponível em:
- **HTTPS**: `https://localhost:7272`
- **HTTP**: `http://localhost:5297`
- **Swagger UI**: `https://localhost:7272/swagger`

---

## 2️⃣ Abrir o Frontend

### Opção A: Servir com um servidor HTTP local

```bash
cd frontend
python -m http.server 8000
# ou com Node.js:
npx http-server
```

Acesse: `http://localhost:8000/Login.html`

### Opção B: Abrir diretamente no navegador

Abra `frontend/Login.html` no navegador (arquivo local)

> ⚠️ Nota: Se abrir como arquivo local (`file://`), o CORS pode bloquear requisições. Use um servidor HTTP local.

---

## 3️⃣ Testar Integração

### 1. Faça login:
- **Login**: `admin`
- **Senha**: `senha123`

### 2. Navegue entre as páginas:
- ✅ Dashboard - Carrega dados do backend
- ✅ Promotores - CRUD completo
- ✅ Empresas - CRUD completo  
- ✅ Registro de Ponto - Entrada/Saída
- ✅ Relatórios - Filtros e gráficos
- ✅ Exportar - Download CSV

---

## 📋 Endpoints Disponíveis

### Autenticação
- `POST /api/auth/login` - Login

### Promotores
- `GET /api/promotores` - Listar todos
- `GET /api/promotores/{id}` - Obter um
- `POST /api/promotores` - Criar
- `PUT /api/promotores/{id}` - Atualizar
- `DELETE /api/promotores/{id}` - Deletar

### Empresas
- `GET /api/empresas` - Listar
- `GET /api/empresas/{id}` - Obter
- `POST /api/empresas` - Criar
- `PUT /api/empresas/{id}` - Atualizar
- `DELETE /api/empresas/{id}` - Deletar

### Dashboard
- `GET /api/dashboard/hoje` - Dados de hoje
- `GET /api/dashboard/visitassemana` - Visitas da semana

### Registros de Acesso
- `GET /api/registrosacessos` - Listar
- `POST /api/registrosacessos/entrada` - Registrar entrada
- `POST /api/registrosacessos/{id}/saida` - Registrar saída

### Relatórios
- `GET /api/relatorios` - Gerar relatório
- `GET /api/relatorios/exportar` - Exportar CSV

---

## 🔑 Estrutura de Token JWT

O token JWT é armazenado em `localStorage` com a chave `token` após login.

Todas as requisições (exceto login) incluem o header:
```
Authorization: Bearer {token}
```

---

## ⚙️ Configurações

### Backend (`appsettings.json`)

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

### Frontend (`js/api.js`)

```javascript
const API_BASE_URL = 'https://localhost:7272/api';
```

---

## 🐛 Troubleshooting

### Erro: CORS bloqueado
- Certifique-se de que o backend está rodando
- Use um servidor HTTP local para o frontend (não `file://`)
- Verifique se CORS está ativado no `Program.cs`

### Erro: Token expirado
- Faça login novamente
- O token expira em 1440 minutos (24 horas)

### Banco de dados não encontrado
- Execute migrations: `dotnet ef database update`
- Ou crie tabelas manualmente usando `BD/PromoterAccessControlDB.sql`

### Porta já em uso
Mude a porta em `launchSettings.json`:
```json
"applicationUrl": "https://localhost:7273;http://localhost:5298"
```

---

## 📁 Estrutura de Arquivos

```
PromoterAccessControl/
├── ControlePromotores.Api/    ← Backend (.NET)
│   ├── Program.cs
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   └── DTOs/
│
└── frontend/                   ← Frontend (HTML/CSS/JS)
    ├── Login.html
    ├── Dashboard.html
    ├── Promotores.html
    ├── Empresas.html
    ├── Registro-ponto.html
    ├── Relatorios.html
    ├── Exportar.html
    ├── css/
    ├── js/
    │   ├── api.js           ← Funções HTTP
    │   ├── auth.js          ← Autenticação
    │   ├── login.js         ← Login
    │   ├── dashboard.js     ← Dashboard
    │   ├── promotores.js    ← CRUD Promotores
    │   ├── empresas.js      ← CRUD Empresas
    │   ├── registro-ponto.js← Registro ponto
    │   ├── relatorios.js    ← Relatórios
    │   └── exportar.js      ← Exportar CSV
    └── img/
```

---

## ✅ Checklist de Integração

- [x] CORS configurado no backend
- [x] JWT implementado
- [x] API.js com funções HTTP (GET, POST, PUT, DELETE)
- [x] Auth.js com login funcional
- [x] Login.html com formulário conectado
- [x] Dashboard.html carregando dados
- [x] Promotores.html com CRUD
- [x] Empresas.html com CRUD
- [x] Registro-ponto.html com entrada/saída
- [x] Relatorios.html com filtros
- [x] Exportar.html com download CSV

---

## 🎯 Próximas Melhorias

- [ ] Refresh token automático
- [ ] Tratamento de erro mais detalhado
- [ ] Validação de formulários no frontend
- [ ] Cache de dados
- [ ] Offline mode
- [ ] PWA (Progressive Web App)

---

**Versão**: 1.0  
**Última atualização**: 22 de Abril de 2026
