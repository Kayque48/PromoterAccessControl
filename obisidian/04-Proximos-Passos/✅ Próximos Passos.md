# ✅ Próximos Passos

> Backlog técnico do projeto. Organizado por prioridade e área.
> Marque com `[x]` quando concluir.

---

## 🔴 Alta Prioridade (necessário para entrega)

### Backend

- [ ] **Migrar para MySQL** — trocar `UseSqlite` por `UseMySql` no `Program.cs` e testar todas as migrations
- [ ] **Proteger `/api/auth/register`** — adicionar `[Authorize(Roles = "admin")]` para evitar criação pública de usuários
- [ ] **Validação de CPF** — implementar validação de CPF no `PromotorDto` (formato e dígitos verificadores)
- [ ] **Migrations formais** — executar `dotnet ef migrations add InitialCreate` e commitar a pasta `Migrations/`
- [ ] **Tratamento de erro padronizado** — criar middleware de exceções global (retornar JSON padronizado em vez de stack trace)

### Frontend

- [ ] **Validação de formulários** — campos obrigatórios, formato de CPF/email, feedback visual de erro
- [ ] **Feedback de carregamento** — usar `ui.js` de forma consistente (loading spinner em todas as operações)
- [ ] **Tratamento de token expirado** — redirecionar para login automaticamente ao receber 401

---

## 🟡 Média Prioridade (qualidade)

### Backend

- [ ] **Refresh token** — implementar endpoint `/api/auth/refresh` com token de renovação de longa duração
- [ ] **Paginação** — adicionar `?page=1&pageSize=20` nos endpoints de listagem (Promotores, Registros, Relatórios)
- [ ] **Filtros adicionais** — busca por nome, CPF, empresa nos endpoints de promotores
- [ ] **CORS restrito** — em produção, substituir `AllowAll` pela URL real do frontend
- [ ] **Logs estruturados** — adicionar Serilog ou similar para logar erros em arquivo/banco
- [ ] **Testes unitários** — cobrir pelo menos `TokenService`, `RegistroAcessoService` e validação de bitmask

### Frontend

- [ ] **Confirmação antes de deletar** — modal de confirmação em ações destrutivas
- [ ] **Paginação nas tabelas** — implementar navegação de páginas em Promotores e Relatórios
- [ ] **Responsividade mobile** — ajustar CSS para telas menores (celular)
- [ ] **Exportar com filtros ativos** — Exportar.html deve herdar filtros de Relatorios.html

---

## 🟢 Baixa Prioridade (melhorias futuras)

- [ ] **Dashboard com gráficos** — integrar Chart.js para visualização de visitas por semana
- [ ] **Upload de documentos** — implementar `PromotorDocumento` no frontend (envio de arquivo)
- [ ] **Notificações** — alertar admin quando promotor não registrar saída após X horas
- [ ] **PWA** — tornar o frontend instalável como Progressive Web App
- [ ] **Modo offline** — cache local de dados para uso sem internet
- [ ] **Perfil de usuário** — tela para trocar senha e editar dados do próprio usuário
- [ ] **Multi-idioma** — suporte a inglês (já existe `README_EN.md`)

---

## 📋 Checklist de Entrega TCC

- [ ] Banco de dados rodando em MySQL (não SQLite)
- [ ] Migrations commitadas no repositório
- [x] `appsettings.Development.json` no `.gitignore` ✅ (já está)
- [x] README completo ✅ (já está)
- [x] Swagger documentado ✅ (já está)
- [x] CRUD de Promotores funcional ✅
- [x] CRUD de Empresas funcional ✅
- [x] Registro de entrada/saída funcional ✅
- [x] Autenticação JWT funcional ✅
- [x] Dashboard com dados reais ✅
- [x] Relatório com filtros ✅
- [x] Exportação CSV ✅
- [x] Validação de dias permitidos (bitmask) ✅
- [ ] Validações no frontend [ ]
- [ ] Testes documentados [ ]
- [ ] Apresentação preparada [ ]

---

## 🐛 Bugs Conhecidos

→ Ver [[🐛 Bugs e Problemas Conhecidos]]
