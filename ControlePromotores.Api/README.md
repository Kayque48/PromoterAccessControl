# Controle de Promotores - API ASP.NET Core

Backend completo para sistema de controle de promotores, com autenticação JWT, CRUD de empresas e promotores, registro de ponto, dashboard e relatórios.

## 🚀 Quick Start

### Pré-requisitos
- .NET 10.0 ou superior
- MySQL 8.0 ou superior
- Visual Studio Code ou Visual Studio

### 1. Configurar Banco de Dados

Atualize a string de conexão em `appsettings.json`:

```json
"ConnectionStrings": {
  "MySqlConnection": "Server=localhost;Database=ControlePromotores;User Id=root;Password=sua_senha;"
}
```

### 2. Gerar Chave JWT

No `appsettings.json`, altere a chave JWT para uma mais segura:

```json
"Jwt": {
  "Key": "sua-chave-secreta-muito-longa-e-segura-com-minimo-32-caracteres",
  "Issuer": "ControlePromotoresAPI",
  "Audience": "ControlePromotoresClient",
  "ExpirationMinutes": 1440
}
```

### 3. Restaurar Dependências e Executar

```bash
cd ControlePromotores.Api
dotnet restore
dotnet run
```

A API estará disponível em:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

### 4. Credenciais Padrão

```
Login: admin
Senha: admin123
```

> ⚠️ **Altere essas credenciais em produção!**

---

## 📋 Funcionalidades

### ✅ Autenticação (JWT)
- Login com usuário e senha
- Geração de token JWT
- Registro de novo usuário (admin apenas)
- Rastreamento de último login

### ✅ Empresas (CRUD Completo)
- Cadastro com CNPJ, razão social, nome fantasia
- Endereço completo (rua, número, bairro, cidade, estado, CEP)
- Email e telefone
- Soft delete (inativação)

### ✅ Promotores (CRUD Completo)
- Cadastro com CPF, nome, contatos
- Relacionamento com empresa
- Endereço completo
- Data de contratação
- Soft delete (inativação)

### ✅ Registro de Ponto
- Registrar entrada de promotor
- Registrar saída
- Cálculo automático de duração
- Lista de promotores ativos
- Filtros por promotor e empresa

### ✅ Dashboard
- Total de promotores ativos
- Total de visitas hoje
- Média de horas por promotor
- Total de registros últimos 30 dias
- Gráficos: visitas por dia, duração média, ranking por empresa

### ✅ Relatórios
- Filtros por data, empresa e promotor
- Dados agregados
- Exportação em CSV

---

## 🏗️ Arquitetura

### Padrão em Camadas

```
Controllers    → Requisições HTTP
    ↓
Services       → Lógica de negócio
    ↓
Models/DTOs    → Dados
    ↓
DbContext      → Acesso ao banco
```

### Estrutura de Pastas

```
Controllers/
├── AuthController.cs              # Autenticação
├── EmpresasController.cs          # Empresas CRUD
├── PromotoresController.cs        # Promotores CRUD
├── RegistrosAcessoController.cs   # Entrada/Saída
├── DashboardController.cs         # Dashboard
└── RelatoriosController.cs        # Relatórios

Models/
├── Usuario.cs                     # Usuário do sistema
├── Empresa.cs                     # Empresa
├── Promotor.cs                    # Promotor
└── RegistroAcesso.cs              # Registro de ponto

Services/
├── TokenService.cs                # JWT
├── EmpresaService.cs              # Lógica de empresas
├── PromotorService.cs             # Lógica de promotores
├── RegistroAcessoService.cs       # Lógica de pontos
├── DashboardService.cs            # Cálculos
└── RelatorioService.cs            # Relatórios

DTOs/
├── LoginRequest.cs                # Autenticação
├── EmpresaDto.cs                  # Empresas
├── PromotorDto.cs                 # Promotores
├── RegistroAcessoDto.cs           # Pontos
├── DashboardDto.cs                # Dashboard
└── RelatorioDto.cs                # Relatórios

BD/
└── PromotoresContext.cs           # DbContext EF Core
```

---

## 🔌 API Endpoints

### Autenticação
- `POST /api/auth/login` - Login
- `POST /api/auth/register` - Registrar novo usuário

### Empresas
- `GET /api/empresas` - Listar todas
- `GET /api/empresas/{id}` - Buscar por ID
- `POST /api/empresas` - Criar (admin)
- `PUT /api/empresas/{id}` - Atualizar (admin)
- `DELETE /api/empresas/{id}` - Deletar (admin)

### Promotores
- `GET /api/promotores` - Listar todas
- `GET /api/promotores?empresaId={id}` - Filtrar por empresa
- `GET /api/promotores/{id}` - Buscar por ID
- `POST /api/promotores` - Criar (admin)
- `PUT /api/promotores/{id}` - Atualizar (admin)
- `DELETE /api/promotores/{id}` - Deletar (admin)

### Registros de Acesso
- `POST /api/registrosacesso/entrada` - Registrar entrada
- `POST /api/registrosacesso/saida` - Registrar saída
- `GET /api/registrosacesso/ativos` - Promotores ativos
- `GET /api/registrosacesso/promotor/{id}` - Registros por promotor
- `GET /api/registrosacesso/empresa/{id}` - Registros por empresa
- `GET /api/registrosacesso/{id}` - Buscar por ID

### Dashboard
- `GET /api/dashboard/hoje` - Dados de hoje
- `GET /api/dashboard/visitassemana` - Visitas por dia
- `GET /api/dashboard/duraomedia` - Duração média
- `GET /api/dashboard/rankingempresa` - Ranking

### Relatórios
- `POST /api/relatorios/agregado` - Relatório com filtros
- `POST /api/relatorios/exportar-csv` - Exportar CSV

---

## 🔐 Autenticação

Todo request (exceto login) deve incluir:

```
Authorization: Bearer {seu_token_jwt}
```

O token expira em 24 horas (configurável em `appsettings.json`).

---

## 🧪 Testando a API

### Usando REST Client (VSCode)

Abra o arquivo `test-api.http` e use as requisições pré-configuradas.

### Usando cURL

```bash
# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"login":"admin","senha":"admin123"}'

# Listar empresas
curl -X GET http://localhost:5000/api/empresas \
  -H "Authorization: Bearer {token}"
```

### Usando Postman

1. Importe as rotas do arquivo `test-api.http`
2. Faça login para obter o token
3. Use o token em todos os requests subsequentes

---

## 📊 Exemplo: Fluxo Completo

1. **Login**
   ```
   POST /api/auth/login
   Login: admin, Senha: admin123
   → Recebe token JWT
   ```

2. **Criar Empresa**
   ```
   POST /api/empresas
   {cnpj, razaoSocial, nomeFantasia, ...}
   → Retorna ID 1
   ```

3. **Criar Promotor**
   ```
   POST /api/promotores
   {nome, cpf, empresaId: 1, ...}
   → Retorna ID 1
   ```

4. **Registrar Entrada**
   ```
   POST /api/registrosacesso/entrada
   {promotorId: 1}
   → Retorna registro de entrada
   ```

5. **Registrar Saída**
   ```
   POST /api/registrosacesso/saida
   {registroId: 1}
   → Calcula duração automaticamente
   ```

6. **Ver Dashboard**
   ```
   GET /api/dashboard/hoje
   → Mostra totais do dia
   ```

7. **Gerar Relatório**
   ```
   POST /api/relatorios/agregado
   {dataInicio, dataFim, ...}
   → Retorna dados agregados
   ```

---

## ✅ Boas Práticas Implementadas

- ✅ **Autenticação JWT** segura
- ✅ **Soft Delete** (inativação) em vez de exclusão física
- ✅ **Validação de dados** com data annotations
- ✅ **Tratamento de erros** com mensagens claras
- ✅ **Índices únicos** para CNPJ e CPF
- ✅ **Relacionamentos** bem definidos
- ✅ **DTOs** para separar modelos internos de APIs
- ✅ **Services** com lógica de negócio isolada
- ✅ **CORS** configurado para frontend
- ✅ **Swagger/OpenAPI** documentação

---

## 🔧 Configuração Avançada

### Alterar Porta
Em `Properties/launchSettings.json`:
```json
"applicationUrl": "https://localhost:5001;http://localhost:5000"
```

### Database Migrations
```bash
# Criar migration
dotnet ef migrations add NomedaMigracao

# Aplicar migration
dotnet ef database update

# Remover última migration
dotnet ef migrations remove
```

### Logs
Configure o nível de log em `appsettings.json`

---

## 🐛 Troubleshooting

### Erro: "Unable to connect to the database"
- Verifique se MySQL está rodando
- Confirme string de conexão em `appsettings.json`
- Verifique se o usuário MySQL tem as permissões corretas

### Erro: "Invalid JWT"
- Confirme que a chave JWT em `appsettings.json` tem 32+ caracteres
- Verifique se o token não expirou
- Confirme que o token está sendo enviado corretamente

### Erro: "CORS policy blocked this request"
- Configure CORS em `Program.cs` se necessário
- Adicione o domínio do frontend em `appsettings.json`

---

## 📝 Licença

Este projeto é fornecido como está para fins educacionais.

---

## 👤 Autor

Desenvolvido como sistema de controle de promotores.

---

## 📚 Referências

- [Microsoft Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core JWT](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [RESTful API Design](https://restfulapi.net/)

---

**Para documentação completa das rotas, veja [API_DOCUMENTATION.md](./API_DOCUMENTATION.md)**

