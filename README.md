# Controle de Promotores

Sistema para controle de entrada/saída de promotores em empresas, com dashboard, relatórios e exportação.

## Funcionalidades implementadas

- Autenticação via JWT
- Cadastro/edição/desativação de promotores
- Cadastro de empresas e listagem de promotores por empresa
- Registro de ponto (entrada/saída) com validação de dias permitidos
- Dashboard com indicadores e gráficos:
  - Visitas por dia da semana
  - Tempo médio por empresa
  - Distribuição por empresa
- Relatórios com filtros por data, empresa, promotor e busca por nome
- Exportação para CSV do histórico gerado
- Verificação de dia correto de visita baseado em dias autorizados por promotor
- Gestão de Empresas com CNPJ/razão social/nome fantasia, telefone e email corporativo
- Cadastro de Promotores com CPF, categoria, CEP/ViaCEP, logradouro/numero/complemento, dias permitidos, telefone/email
- Busca de endereço automático via ViaCEP (CEP -> logradouro)
- Validação de formulário com feedback visual de erro (is-invalid / invalid-feedback) e Focus acessível
- Melhorias UX: cartas/inputs mais espaçados, modal maior, contraste AAA, navegação por teclado, labels/accessibilidade

## Estrutura de arquivos

- `frontend/` - páginas e scripts do front-end (HTML/CSS/JS)
- `Controladores/` - APIs do back-end (ASP.NET Core)
- `Models/` - classes de modelo (Promotor, Empresa, RegistroAcesso)
- `BD/` - contexto EF Core

## Como executar

1. Certifique-se de ter .NET 10 SDK instalado.
2. Abra o terminal na pasta do projeto:
   ```bash
   cd d:\ControlePromotores.Api
   ```
3. Restaure dependências:
   ```bash
   dotnet restore
   ```
4. Execute a aplicação:
   ```bash
   dotnet run --project ControlePromotores.Api.csproj
   ```
5. Abra no navegador `http://localhost:5000` (ou porta mostrada no terminal).

## Arquivos modificados recentemente

- `frontend/Dashboard.html`
- `frontend/js/dashboard.js`
- `frontend/Promotores.html`
- `frontend/js/promotores.js`
- `frontend/Registro-ponto.html`
- `frontend/js/registro-ponto.js`
- `frontend/Relatorios.html`
- `frontend/js/relatorios.js`
- `frontend/Exportar.html`
- `frontend/js/exportar.js`
- `Controladores/DashboardController.cs`
- `Controladores/ControladorPromotores.cs`
- `Controladores/ControladorRegistros.cs`
- `Models/Promotor.cs`

## Push para GitHub

```bash
git add .
git commit -m "Atualiza projeto: dashboard, promotores, registros e exportação"
git pull origin main --rebase
git push origin main
```

