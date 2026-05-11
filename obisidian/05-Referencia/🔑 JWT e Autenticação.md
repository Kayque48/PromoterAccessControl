# 🔑 Referência — JWT e Autenticação

> Como funciona o sistema de autenticação do projeto.

---

## Fluxo Completo

```
1. Usuário envia login/senha
   POST /api/auth/login
   { "login": "admin", "senha": "senha123" }

2. API busca usuário no banco (por Login + Ativo = true)

3. BCrypt.Verify(senhaDigitada, senhaHashNoBanco)
   ✅ Match → gera JWT
   ❌ No match → 401 Unauthorized

4. JWT gerado com claims:
   - NameIdentifier = usuarioId
   - Name = login
   - Role = perfil (admin/usuario)
   - Expira em: agora + ExpirationMinutes (1440 = 24h)

5. Frontend salva token em localStorage
   localStorage.setItem('token', data.token)

6. Requisições subsequentes incluem:
   Authorization: Bearer eyJhbGci...
```

---

## Estrutura do Token JWT

```
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload (Claims):
{
  "nameid": "1",
  "unique_name": "admin",
  "role": "admin",
  "nbf": 1715000000,
  "exp": 1715086400,   ← timestamp de expiração
  "iat": 1715000000
}

Signature: HMACSHA256(base64(header) + "." + base64(payload), secret)
```

---

## Configuração no appsettings.json

```json
"Jwt": {
  "Key": "sua-chave-secreta-muito-longa-e-segura-aqui-min-32-caracteres",
  "Issuer": "ControlePromotoresAPI",
  "Audience": "ControlePromotoresClient",
  "ExpirationMinutes": 1440
}
```

> ⚠️ A chave deve ter **no mínimo 32 caracteres** (256 bits). Se for menor, o JWT Bearer vai rejeitar na inicialização.

---

## Proteger um endpoint

```csharp
// Qualquer usuário logado:
[Authorize]
[HttpGet]
public IActionResult MeuEndpoint() { ... }

// Apenas admins:
[Authorize(Roles = "admin")]
[HttpDelete("{id}")]
public IActionResult DeletarAlgo(int id) { ... }

// Endpoint público (sem token):
[AllowAnonymous]
[HttpPost("login")]
public IActionResult Login() { ... }
```

---

## BCrypt — Hash de Senha

```csharp
// Criar hash (ao registrar usuário):
string hash = BCrypt.Net.BCrypt.HashPassword("senha123");
// resultado: "$2a$10$xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"

// Verificar senha (ao fazer login):
bool valido = BCrypt.Net.BCrypt.Verify("senha123", hash);
// true = senha correta
```

> O custo padrão (10) significa ~100ms por hash — suficiente para dificultar brute force.
