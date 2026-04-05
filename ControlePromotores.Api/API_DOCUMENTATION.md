# API Controle de Promotores - Documentação

## Início Rápido

### 1. Configuração do Banco de Dados

**appsettings.json** (altere as credenciais conforme necessário):
```json
"ConnectionStrings": {
  "MySqlConnection": "Server=localhost;Database=ControlePromotores;User Id=root;Password=sua_senha;"
}
```

### 2. Chave JWT

Gere uma chave segura (mínimo 32 caracteres) e configure em **appsettings.json**:
```json
"Jwt": {
  "Key": "sua-chave-secreta-muito-longa-e-segura-aqui-min-32-caracteres",
  "Issuer": "ControlePromotoresAPI",
  "Audience": "ControlePromotoresClient",
  "ExpirationMinutes": 1440
}
```

### 3. Executar a API

```bash
dotnet run
```

A API estará disponível em: `http://localhost:5000` ou `https://localhost:5001`

---

## Rotas da API

### 🔐 Autenticação

#### Login
```
POST /api/auth/login
Content-Type: application/json

{
  "login": "admin",
  "senha": "admin123"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "usuarioId": 1,
  "nome": "Administrador",
  "perfil": "admin"
}
```

#### Registrar Usuário (Admin only)
```
POST /api/auth/register
Authorization: Bearer {token}
Content-Type: application/json

{
  "nome": "Novo Usuário",
  "login": "usuario1",
  "senha": "senha123",
  "perfil": "usuario"
}
```

---

### 🏢 Empresas

#### Listar Todas
```
GET /api/empresas
Authorization: Bearer {token}
```

#### Buscar por ID
```
GET /api/empresas/{id}
Authorization: Bearer {token}
```

#### Criar (Admin only)
```
POST /api/empresas
Authorization: Bearer {token}
Content-Type: application/json

{
  "cnpj": "12.345.678/0001-90",
  "razaoSocial": "Empresa LTDA",
  "nomeFantasia": "Empresa XYZ",
  "telefone": "1133334444",
  "email": "contato@empresa.com.br",
  "endereco": "Rua A",
  "numero": "123",
  "complemento": "Sala 1",
  "bairro": "Centro",
  "cidade": "São Paulo",
  "estado": "SP",
  "cep": "01234-567"
}
```

#### Atualizar (Admin only)
```
PUT /api/empresas/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "cnpj": "12.345.678/0001-90",
  "razaoSocial": "Empresa LTDA",
  "nomeFantasia": "Empresa XYZ",
  ...
}
```

#### Deletar (Admin only)
```
DELETE /api/empresas/{id}
Authorization: Bearer {token}
```

---

### 👔 Promotores

#### Listar Todos (com filtro opcional por empresa)
```
GET /api/promotores?empresaId=1
Authorization: Bearer {token}
```

#### Buscar por ID
```
GET /api/promotores/{id}
Authorization: Bearer {token}
```

#### Criar (Admin only)
```
POST /api/promotores
Authorization: Bearer {token}
Content-Type: application/json

{
  "nome": "João Silva",
  "cpf": "123.456.789-00",
  "telefone": "11987654321",
  "email": "joao@example.com",
  "endereco": "Rua B",
  "numero": "456",
  "complemento": "Apt 101",
  "bairro": "Vila",
  "cidade": "São Paulo",
  "estado": "SP",
  "cep": "01234-567",
  "empresaId": 1
}
```

#### Atualizar (Admin only)
```
PUT /api/promotores/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "nome": "João Silva",
  "cpf": "123.456.789-00",
  ...
}
```

#### Deletar (Admin only)
```
DELETE /api/promotores/{id}
Authorization: Bearer {token}
```

---

### ⏰ Registros de Acesso

#### Registrar Entrada
```
POST /api/registrosacesso/entrada
Authorization: Bearer {token}
Content-Type: application/json

{
  "promotorId": 1
}

Response:
{
  "id": 1,
  "entrada": "2024-04-05T10:30:00Z",
  "saida": null,
  "tempoPermanenciaMinutos": null,
  "promotorId": 1,
  "promotorNome": "João Silva"
}
```

#### Registrar Saída
```
POST /api/registrosacesso/saida
Authorization: Bearer {token}
Content-Type: application/json

{
  "registroId": 1
}

Response:
{
  "id": 1,
  "entrada": "2024-04-05T10:30:00Z",
  "saida": "2024-04-05T12:00:00Z",
  "tempoPermanenciaMinutos": 90,
  "promotorId": 1,
  "promotorNome": "João Silva"
}
```

#### Listar Promotores Ativos (sem saída)
```
GET /api/registrosacesso/ativos
Authorization: Bearer {token}
```

#### Buscar Registros por Promotor
```
GET /api/registrosacesso/promotor/{promotorId}?dataInicio=2024-04-01&dataFim=2024-04-05
Authorization: Bearer {token}
```

#### Buscar Registros por Empresa
```
GET /api/registrosacesso/empresa/{empresaId}?dataInicio=2024-04-01&dataFim=2024-04-05
Authorization: Bearer {token}
```

#### Buscar Registro por ID
```
GET /api/registrosacesso/{id}
Authorization: Bearer {token}
```

---

### 📊 Dashboard

#### Dados do Dia (Promotores ativos, visitas hoje, média de horas)
```
GET /api/dashboard/hoje
Authorization: Bearer {token}

Response:
{
  "totalPromotoresAtivos": 5,
  "totalVisitasHoje": 12,
  "mediaHorasPorPromotor": 2.5,
  "totalRegistrosUltimos30Dias": 250
}
```

#### Visitas por Dia (Última semana)
```
GET /api/dashboard/visitassemana
Authorization: Bearer {token}

Response:
[
  {
    "data": "2024-03-31T00:00:00Z",
    "totalVisitas": 10
  },
  ...
]
```

#### Duração Média por Promotor
```
GET /api/dashboard/duraomedia?limitePromotores=10
Authorization: Bearer {token}
```

#### Ranking de Visitas por Empresa
```
GET /api/dashboard/rankingempresa
Authorization: Bearer {token}
```

---

### 📄 Relatórios

#### Obter Relatório Agregado
```
POST /api/relatorios/agregado
Authorization: Bearer {token}
Content-Type: application/json

{
  "dataInicio": "2024-04-01T00:00:00Z",
  "dataFim": "2024-04-05T23:59:59Z",
  "empresaId": null,
  "promotorId": null
}

Response:
{
  "totalRegistros": 50,
  "duracaoMediaMinutos": 125.5,
  "promotoresUnicos": 5,
  "empresasUnicos": 2,
  "registros": [...]
}
```

#### Exportar em CSV
```
POST /api/relatorios/exportar-csv
Authorization: Bearer {token}
Content-Type: application/json

{
  "dataInicio": "2024-04-01T00:00:00Z",
  "dataFim": "2024-04-05T23:59:59Z",
  "empresaId": null,
  "promotorId": null
}

Response: [Download do arquivo CSV]
```

---

## Códigos de Status HTTP

- **200 OK**: Requisição bem-sucedida
- **201 Created**: Recurso criado com sucesso
- **204 No Content**: Recurso deletado com sucesso
- **400 Bad Request**: Erro de validação
- **401 Unauthorized**: Não autenticado
- **403 Forbidden**: Sem permissão
- **404 Not Found**: Recurso não encontrado
- **500 Internal Server Error**: Erro do servidor

---

## Estrutura do Projeto

```
Controllers/
├── AuthController.cs           # Autenticação e login
├── EmpresasController.cs       # CRUD de empresas
├── PromotoresController.cs     # CRUD de promotores
├── RegistrosAcessoController.cs # Entrada/saída
├── DashboardController.cs      # Dados do dashboard
└── RelatoriosController.cs     # Relatórios e exportação

Models/
├── Usuario.cs                  # Usuário do sistema
├── Empresa.cs                  # Dados de empresa
├── Promotor.cs                 # Dados de promotor
└── RegistroAcesso.cs           # Registro de entrada/saída

Services/
├── TokenService.cs             # Geração de JWT
├── EmpresaService.cs           # Lógica de empresas
├── PromotorService.cs          # Lógica de promotores
├── RegistroAcessoService.cs    # Lógica de registros
├── DashboardService.cs         # Cálculos de dashboard
└── RelatorioService.cs         # Geração de relatórios

DTOs/
├── LoginRequest.cs             # Request/Response de login
├── EmpresaDto.cs               # DTOs de empresa
├── PromotorDto.cs              # DTOs de promotor
├── RegistroAcessoDto.cs        # DTOs de registro
├── DashboardDto.cs             # DTOs de dashboard
└── RelatorioDto.cs             # DTOs de relatório

BD/
├── PromotoresContext.cs        # DbContext do EF Core
```

---

## Autenticação com JWT

Toda requisição (exceto login) deve incluir o header:
```
Authorization: Bearer {token}
```

O token é gerado após login bem-sucedido e expira em 24h (configurável).

---

## Boas Práticas

✅ Use soft delete (Ativo = false) em vez de deletar registros  
✅ Valide sempre CNPJ e CPF duplicados  
✅ Registre tentativas de login  
✅ Use HTTPS em produção  
✅ Altere a chave JWT padrão  
✅ Configure CORS adequadamente  
✅ Implemente rate limiting se necessário  

---

## Troubleshooting

**Erro de conexão com banco de dados**
- Verifique se MySQL está rodando
- Confirme credenciais em appsettings.json
- Verifique se o banco "ControlePromotores" existe

**Erro 401 Unauthorized**
- Verifique se o token foi enviado no header Authorization
- Verifique se o token não expirou
- Confirme se a chave JWT em appsettings.json está correta

**Erro 403 Forbidden**
- Verifique se o usuário tem a permissão correta (perfil)
- Admin pode fazer qualquer operação
- Usuário comum pode apenas ler dados

---

## Suporte

Para dúvidas ou sugestões, abra uma issue no repositório.
