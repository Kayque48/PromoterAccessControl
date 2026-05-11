# 🖥️ Frontend — Telas e Arquivos

> Frontend em HTML/CSS/JS puro. Sem framework. Roda em qualquer navegador.
> **Ponto de entrada:** `frontend/Login.html`

---

## Mapa de Navegação das Telas

```
Login.html
    │
    ▼ (autenticado)
Dashboard.html ──── Promotores.html
    │                     │
    ├─── Empresas.html     └── [modal] Criar/Editar Promotor
    │
    ├─── Registro-ponto.html
    │         └── Seleciona promotor → Registra Entrada/Saída
    │
    ├─── Relatorios.html
    │         └── Filtros → Tabela de resultados
    │
    └─── Exportar.html
              └── Filtros → Download CSV
```

---

## Telas e Responsabilidades

### `Login.html`
Formulário de login. Chama `POST /api/auth/login`, salva o token em `localStorage`.
- JS: `js/login.js` + `js/auth.js`
- CSS: `css/login.css`

### `Dashboard.html`
Visão geral do dia. Cards com promotores presentes, entradas/saídas, gráfico de visitas da semana.
- JS: `js/dashboard.js`
- CSS: `css/dashboard.css`
- Endpoints: `GET /api/dashboard/hoje`, `GET /api/dashboard/visitassemana`

### `Promotores.html`
CRUD completo. Tabela com busca, botões de editar/desativar, modal de criação.
- JS: `js/promotores.js`
- Endpoints: `GET`, `POST`, `PUT`, `DELETE /api/promotores`

### `Empresas.html`
CRUD completo de empresas.
- JS: `js/empresas.js`
- CSS: `css/empresas.css`
- Endpoints: `GET`, `POST`, `PUT`, `DELETE /api/empresas`

### `Registro-ponto.html`
Seleciona promotor e empresa, registra entrada ou saída.
- JS: `js/registro-ponto.js`
- Endpoints: `POST /api/registrosacessos/entrada`, `POST /api/registrosacessos/{id}/saida`

### `Relatorios.html`
Filtros por período, promotor e empresa. Exibe tabela de registros.
- JS: `js/relatorios.js`
- CSS: `css/relatorios.css`
- Endpoints: `GET /api/relatorios`

### `Exportar.html`
Mesmos filtros do relatório + botão de download em CSV.
- JS: `js/exportar.js`
- CSS: `css/exportar.css`
- Endpoints: `GET /api/relatorios/exportar`

---

## Arquivos JavaScript Principais

### `js/api.js` — Núcleo de comunicação
```javascript
// Configuração base
const API_BASE_URL = 'https://localhost:7272/api';

// Funções disponíveis:
apiGet(endpoint)           // GET com token automático
apiPost(endpoint, body)    // POST com token automático
apiPut(endpoint, body)     // PUT com token automático
apiDelete(endpoint)        // DELETE com token automático
```
> ✅ Já injeta o `Authorization: Bearer {token}` em toda requisição.

### `js/auth.js` — Gerenciamento de autenticação
```javascript
// Funções disponíveis:
getToken()           // Retorna token do localStorage
saveToken(token)     // Salva token
logout()             // Remove token e redireciona para Login
isLoggedIn()         // Verifica se tem token válido
```
> ⚠️ Todas as páginas (exceto Login) chamam `isLoggedIn()` no início. Se não logado, redireciona.

### `js/ui.js` — Utilitários de interface
```javascript
// Funções comuns reutilizadas em todo o frontend
showLoading()
hideLoading()
showError(message)
showSuccess(message)
```

---

## Como Rodar o Frontend

```bash
# Opção 1: Python (mais simples)
cd frontend
python3 -m http.server 8000

# Opção 2: Node.js
npx http-server frontend -p 8000

# Acesse:
http://localhost:8000/Login.html
```

> ⚠️ Não abrir como `file://` — o CORS bloqueia requisições para a API.

---

## Configurar URL da API

Se a porta do backend mudar, edite **apenas** `js/api.js`:

```javascript
// Linha 1 de api.js
const API_BASE_URL = 'https://localhost:7272/api';
//                                      ^^^^
//                              Mude só aqui
```

---

## CSS — Estrutura

```
css/
├── Style.css        ← Estilos globais e componentes comuns
├── login.css        ← Tela de login
├── dashboard.css    ← Dashboard
├── empresas.css     ← Tela de empresas
├── relatorios.css   ← Tela de relatórios
└── exportar.css     ← Tela de exportar
```

→ Ver como rodar tudo junto em [[🚀 Como Rodar o Projeto]]
