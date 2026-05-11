# 🐛 Bugs e Problemas Conhecidos

> Registro de bugs identificados. Atualizar quando resolver ou descobrir novos.

---

## 🔴 Críticos (impedem funcionamento)

### Nenhum crítico no momento ✅
O projeto compila e roda com SQLite. Ver [[🚀 Como Rodar o Projeto]] se tiver problemas.

---

## 🟡 Médios (afetam experiência)

### `[B-001]` Register sem autenticação
**Arquivo:** `Controllers/AuthController.cs`
**Problema:** `POST /api/auth/register` está público — qualquer um pode criar usuários admin.
**Solução:**
```csharp
// Adicionar antes do método Register:
[Authorize(Roles = "admin")]
[HttpPost("register")]
public async Task<ActionResult<LoginResponse>> Register(...)
```
**Status:** ⏳ Pendente

---

### `[B-002]` Duplicação de arquivos JS no frontend
**Arquivo:** `frontend/js/`
**Problema:** Existem dois sets de arquivos (maiúsculas e minúsculas): `Api.js` e `api.js`, `Auth.js` e `auth.js`, etc. Em Linux, são arquivos diferentes — pode causar imports quebrados.
**Solução:** Remover os arquivos com inicial maiúscula (`Api.js`, `Auth.js`, etc.) e manter apenas os minúsculos.
```bash
cd frontend/js
rm Api.js Auth.js Dashboard.js Empresas.js Promotores.js Registro-ponto.js Relatorios.js Style.css
```
**Status:** ⏳ Pendente

---

### `[B-003]` Pomelo EF Core vs .NET 10
**Arquivo:** `ControlePromotores.Api.csproj`
**Problema:** `Pomelo.EntityFrameworkCore.MySql 9.0.0` suporta oficialmente até EF Core 9. O projeto usa EF Core 10. Pode gerar warnings ou incompatibilidades ao migrar para MySQL.
**Solução:** Aguardar Pomelo 10.x ou usar `--no-build` nos warnings. Para dev com SQLite, não afeta.
**Status:** ⚠️ Monitorar

---

### `[B-004]` CORS AllowAll em desenvolvimento
**Arquivo:** `Program.cs`
**Problema:** `builder.AllowAnyOrigin()` é inseguro para produção.
**Solução:** Criar política com URL específica do frontend:
```csharp
options.AddPolicy("FrontendPolicy", policy =>
    policy.WithOrigins("http://localhost:8000", "https://seudominio.com")
          .AllowAnyMethod()
          .AllowAnyHeader());
```
**Status:** ⏳ Pendente (ok para desenvolvimento)

---

## 🟢 Menores (cosméticos / baixo impacto)

### `[B-005]` WeatherForecast no Program.cs
**Arquivo:** `backend/Program.cs` (pasta legada)
**Problema:** Resquício do template padrão do .NET (`record WeatherForecast...`). Não causa erro mas é código morto.
**Status:** ✅ Pode remover quando revisar o arquivo

---

### `[B-006]` Seed de dias permitidos fixo
**Arquivo:** `Data/DataInitializer.cs`
**Problema:** Promotores criados pelo seed têm `DiasPermitidos = 62` (Seg-Sex) hardcoded. Não há UI para editar isso.
**Status:** ⏳ Pendente — adicionar campo na tela de edição de promotores

---

## Histórico de Bugs Resolvidos

| ID | Descrição | Resolvido em |
|---|---|---|
| — | `[httpGet]` minúsculo em ControladorPromotores | Branch main inicial |
| — | `AppDbContext` → `PromotoresContext` em ControladorRegistros | Branch main inicial |
| — | Bloco solto fora de método em ControladorAuticacao | Branch main inicial |
| — | `System.componentModel` com 'c' minúsculo em Promotor | Branch main inicial |
| — | `[required]` minúsculo em LoginModel | Branch main inicial |

→ Ver próximos passos em [[✅ Próximos Passos]]
