# 📡 Controllers e Endpoints

> Todos os endpoints disponíveis na API. Use o Swagger em `https://localhost:7272/swagger` para testar interativamente.

**Credenciais padrão (seed):** `admin` / `senha123`

---

## 🔐 AuthController — `/api/auth`

### POST `/api/auth/login`
Autentica o usuário e retorna o JWT.

```
Body (JSON):
{
  "login": "admin",
  "senha": "senha123"
}

Resposta 200:
{
  "token": "eyJhbGci...",
  "usuarioId": 1,
  "nome": "Administrador",
  "perfil": "admin"
}

Resposta 401: Login ou senha inválidos
```

### POST `/api/auth/register`
Cria novo usuário. ⚠️ *Ainda sem `[Authorize]` — proteger antes de ir para produção.*

```
Body (JSON):
{
  "nome": "Novo Usuário",
  "login": "novo",
  "senha": "senha123",
  "perfil": "usuario"
}
```

---

## 👤 PromotoresController — `/api/promotores`
🔒 Requer token JWT em todos os endpoints.

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/promotores` | Lista todos os promotores |
| GET | `/api/promotores/{id}` | Retorna um promotor específico |
| POST | `/api/promotores` | Cria novo promotor |
| PUT | `/api/promotores/{id}` | Atualiza dados do promotor |
| DELETE | `/api/promotores/{id}` | Desativa promotor (soft delete) |

```
Exemplo POST /api/promotores:
{
  "cpf": "12345678901",
  "nome": "João Silva",
  "telefone": "19991234567",
  "email": "joao@email.com",
  "tipo": "promotor"
}

Tipos de promotor:
  "promotor"   → pode trabalhar em múltiplas empresas
  "exclusivo"  → vinculado a uma empresa só
```

---

## 🏢 EmpresasController — `/api/empresas`
🔒 Requer token JWT.

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/empresas` | Lista todas as empresas |
| GET | `/api/empresas/{id}` | Retorna uma empresa |
| POST | `/api/empresas` | Cria nova empresa |
| PUT | `/api/empresas/{id}` | Atualiza empresa |
| DELETE | `/api/empresas/{id}` | Remove empresa |

---

## ⏱️ RegistrosAcessoController — `/api/registrosacessos`
🔒 Requer token JWT.

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/registrosacessos` | Lista registros (com filtros) |
| POST | `/api/registrosacessos/entrada` | Registra entrada do promotor |
| POST | `/api/registrosacessos/{id}/saida` | Registra saída e calcula permanência |

```
Fluxo de registro de ponto:

1. POST /api/registrosacessos/entrada
   Body: { "promotorId": 1, "empresaId": 2 }
   → Cria registro com Tipo="entrada", DataHora=now
   → Valida se promotor está ativo na empresa
   → Valida DiasPermitidos (bitmask)

2. POST /api/registrosacessos/1/saida
   → Atualiza registro com Tipo="saida"
   → Calcula PermanenciaMin = (saida - entrada).TotalMinutes
```

**Bitmask de dias permitidos:**
```
1  = Domingo
2  = Segunda
4  = Terça
8  = Quarta
16 = Quinta
32 = Sexta
64 = Sábado

Exemplos:
62  = Seg-Sex (padrão)
127 = Todos os dias
96  = Sexta + Sábado
```

---

## 📊 DashboardController — `/api/dashboard`
🔒 Requer token JWT.

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/dashboard/hoje` | Promotores presentes hoje, total de entradas |
| GET | `/api/dashboard/visitassemana` | Contagem de visitas por dia da semana |

```
Resposta /api/dashboard/hoje:
{
  "promotoresPresentes": 5,
  "totalEntradas": 12,
  "totalSaidas": 7,
  "mediaPermanenciaMin": 145
}
```

---

## 📋 RelatoriosController — `/api/relatorios`
🔒 Requer token JWT.

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/relatorios` | Relatório filtrado por período/promotor/empresa |
| GET | `/api/relatorios/exportar` | Exporta registros filtrados em CSV |

```
Query params disponíveis:
  ?dataInicio=2026-01-01
  ?dataFim=2026-05-09
  ?promotorId=1
  ?empresaId=2
```

---

## 🔑 Como usar o token nas requisições

```javascript
// Em api.js — padrão já implementado no frontend
const response = await fetch(`${API_BASE_URL}/promotores`, {
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('token')}`,
    'Content-Type': 'application/json'
  }
});
```

→ Ver configuração do frontend em [[🖥️ Frontend — Telas e Arquivos]]
