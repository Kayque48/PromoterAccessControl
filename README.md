> 🇺🇸 [English Version](README_EN.md)

# Controle de Promotores

## 📋 Visão Geral

Sistema acadêmico para gerenciamento de acesso e permanência de promotores em empresas, desenvolvido como trabalho de conclusão de curso (TCC) em Análise e Desenvolvimento de Sistemas. O projeto demonstra arquitetura modular, integração com banco de dados relacional, autenticação com JWT e geração de relatórios.

---

## 🎯 Objetivo

Centralizar o registro de acesso de promotores, automatizando:

- Registro de entrada e saída
- Cálculo da duração de permanência
- Monitoramento de promotores ativos em tempo real
- Geração de relatórios e métricas operacionais
- Exportação de dados para análise

---

## 🛠️ Stack Tecnológico

| Camada | Tecnologia |
|--------|-----------|
| **Backend** | ASP.NET Core (C#) |
| **Banco de Dados** | MySQL (banco oficial) / SQLite (apoio local de desenvolvimento) |
| **Frontend** | HTML5, CSS3, JavaScript vanilla |
| **Autenticação** | JWT Bearer Token |
| **ORM** | Entity Framework Core |
| **Segurança** | BCrypt (hash de senhas) |

---

## 📁 Estrutura do Projeto

```
PromoterAccessControl/
├── backend/
│   └── ControlePromotores.Api/
│       ├── Controllers/          # Endpoints da API
│       ├── Services/             # Lógica de negócio
│       ├── Models/               # Entidades do domínio
│       ├── DTOs/                 # Contratos de requisição/resposta
│       ├── BD/                   # DbContext
│       ├── Data/                 # Seed de dados
│       ├── Utils/                # Utilitários
│       └── Program.cs            # Configuração da aplicação
├── frontend/
│   ├── css/                      # Estilos
│   ├── js/                       # Scripts (API client, lógica, UI)
│   ├── img/                      # Imagens
│   └── *.html                    # Páginas
├── database/
│   └── PromoterAccessControlDB.sql  # Schema MySQL oficial
└── docs/
    ├── visao-geral.md
    ├── arquitetura.md
    ├── fluxo-funcional.md
    ├── seguranca-e-lgpd.md
    └── pendencias-tecnicas.md
```

---

## ⚙️ Arquitetura

O sistema segue uma arquitetura em **três camadas**:

```
┌─────────────────────────────────────┐
│  Frontend (HTML/CSS/JS)             │
│  - Interface com usuário            │
│  - Consumo de endpoints             │
└─────────────┬───────────────────────┘
              │ HTTP/REST
              ↓
┌─────────────────────────────────────┐
│  Backend (ASP.NET Core)             │
│  - Controllers (exposição)          │
│  - Services (negócio)               │
│  - Models (entidades)               │
│  - DTOs (contrato)                  │
└─────────────┬───────────────────────┘
              │ SQL
              ↓
┌─────────────────────────────────────┐
│  Banco de Dados (MySQL)             │
│  - Persistência                     │
│  - Relacionamentos                  │
└─────────────────────────────────────┘
```

### Padrão de Desenvolvimento

- **Separação em camadas:** Controllers → Services → Models/DTOs → DbContext
- **Autenticação:** JWT com validação por roles/perfis
- **Validação:** Realizada em Services e DTOs
- **Persistência:** Entity Framework Core com navegação automática

---

## ✨ Funcionalidades Implementadas

### Autenticação
- Login com username e password
- Geração e validação de JWT
- Controle de acesso por perfil (Admin, Usuário)

### Cadastros
- **Empresas:** Criar, editar, listar, excluir
- **Promotores:** Criar, editar, listar, vincular a empresa, controlar exclusividade

### Registro de Ponto
- **Entrada:** Registra o horário de chegada do promotor, considerando a empresa vinculada
- **Saída:** Registra horário de partida e calcula permanência automática
- **Ativos:** Lista em tempo real de promotores com entrada registrada e sem saída

### Relatórios e Análises
- **Dashboard:** Métricas operacionais (promotores ativos, empresas ativas, etc.)
- **Relatórios:** Filtrados por período, empresa e promotor
- **Exportação:** Gera arquivo com dados em formato tabulado

---

## 🚀 Como Executar

### Pré-requisitos

- **.NET SDK 10.0+** instalado
- **Node.js** (opcional, apenas para servir frontend)
- **MySQL Server** (banco oficial) ou deixar SQLite como fallback

### 1️⃣ Preparar o Banco de Dados

#### Opção A: MySQL (Recomendado - Banco Oficial)

```bash
# No MySQL:
mysql -u root -p < database/PromoterAccessControlDB.sql
```

Atualize a connection string em `appsettings.Development.json`:
```json
"MySqlConnection": "Server=localhost;Port=3306;Database=promoter_checkin;User Id=root;Password=SEU_PASSWORD;"
```

#### Opção B: SQLite (Desenvolvimento Offline)

Em `appsettings.Development.json`, configure:
```json
"UseSqlite": true
```

### 2️⃣ Executar o Backend

```bash
cd backend/ControlePromotores.Api

# Restaurar dependências
dotnet restore

# Executar em modo desenvolvimento
dotnet run
```

O backend estará disponível em:
- **HTTP:** `http://localhost:5297`
- **HTTPS:** `https://localhost:7272`
- **Swagger:** `http://localhost:5297/swagger/index.html`

### 3️⃣ Executar o Frontend

Serve o frontend usando qualquer server HTTP local (ex: http-server, Live Server no VS Code, Python http.server, etc):

```bash
cd frontend

# Opção 1: http-server (npm)
npx http-server -p 8000

# Opção 2: Python
python -m http.server 8000

# Opção 3: Node http-server
npm install -g http-server
http-server -p 8000
```

Frontend disponível em: `http://localhost:8000`

### ⚙️ Configurações Importantes

**appsettings.Development.json** é um arquivo de configuração **local não versionado**. Exemplo de estrutura:

```json
{
  "UseSqlite": false,
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Port=3306;Database=promoter_checkin;User Id=root;Password=sua_senha;"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-minimo-32-caracteres",
    "Issuer": "ControlePromotoresAPI",
    "Audience": "ControlePromotoresClient",
    "ExpirationMinutes": 1440
  }
}
```

⚠️ **IMPORTANTE:** 
- Nunca versione segredos reais (senhas, chaves JWT) no repositório
- Use variáveis de ambiente ou arquivos locais `.gitignore`
- A chave JWT deve ter **mínimo 32 caracteres**

---

## 📊 Fluxo Principal

1. **Autenticação:** Usuário faz login
2. **Cadastros:** Admin cadastra empresas e promotores
3. **Vinculação:** Promotor é vinculado à empresa (exclusivo ou não)
4. **Entrada:** O promotor registra a chegada e o sistema considera a empresa vinculada ao cadastro
5. **Monitoramento:** Sistema mostra promotores ativos
6. **Saída:** Promotor registra partida (calcula duração automaticamente)
7. **Análise:** Dados visualizáveis em dashboard, relatórios e exportação

---

## 📈 Status Atual do Projeto

### ✅ Implementado e Validado

- Login com autenticação JWT
- CRUD de empresas
- CRUD de promotores com vinculação
- Registro de entrada e saída com cálculo de permanência
- Consulta de promotores ativos
- Relatórios com filtros (período, empresa, promotor)
- Exportação de dados
- Dashboard com métricas básicas
- Validações de entrada (CPF, CNPJ, email, etc.)

### ⚠️ Estado do Dashboard

O dashboard está estabilizado em modo seguro. Os gráficos foram temporariamente desativados como medida de contenção técnica para evitar travamentos, enquanto os indicadores principais permanecem disponíveis.

### 🔄 Identificados para Refinamento Futuro

- Reativação segura de gráficos com melhor tratamento de performance
- Mecanismos avançados de rate limiting e revogação de token
- Testes automatizados (unitários e de integração)
- Refatoração do frontend com estrutura mais modular
- Melhorias na validação de CNPJ nas telas
- Refinamento da regra de promotor exclusivo vs. compartilhado

---

## 🔒 Segurança e Considerações LGPD

### Implementado

✅ Autenticação com JWT  
✅ Hash de senha com BCrypt  
✅ Controle de acesso por perfil  
✅ Validação de entrada em DTOs  

### Limitações Atuais (Conscientes)

⚠️ CORS configurado de forma ampla (AllowAnyOrigin)  
⚠️ Token armazenado em localStorage (vulnerável a XSS)  
⚠️ Ausência de mecanismo de revogação de token  
⚠️ Rate limiting não implementado  
⚠️ Segredos (JWT Key) em arquivo local  

### Dados Pessoais Tratados

O sistema processa: nome, CPF, telefone, email, vínculo com empresa e registros de entrada/saída.

Em ambiente corporativo real, seria necessário:

- Política formal de retenção de dados
- Minimização consciente de dados coletados
- Mascaramento de dados em exibição (ex: CPF parcialmente oculto)
- Controle mais rigoroso de acesso e exportação
- Documentação de base legal (LGPD art. 7)
- Procedimentos de exclusão de dados

O projeto atual demonstra preocupação com segurança básica, mas **não deve ser considerado compliant para produção** sem melhorias adicionais.

---

## 📝 Observações Importantes

1. **Escopo Acadêmico:** Este é um projeto de TCC, com propósitos educacionais. Reflete decisões de design adequadas para demonstração, não necessariamente para ambiente corporativo.

2. **Estrutura Modular:** A arquitetura em camadas facilita manutenção, testes e evolução futura.

3. **Documentação Técnica:** Consulte `docs/` para detalhes de arquitetura, fluxos funcionais e pendências técnicas.

4. **Dados de Exemplo:** O banco é inicializado com dados mínimos para funcionalidade básica.

---

## 📚 Documentação Adicional

- [Visão Geral](docs/visao-geral.md)
- [Arquitetura Técnica](docs/arquitetura.md)
- [Fluxo Funcional](docs/fluxo-funcional.md)
- [Segurança e LGPD](docs/seguranca-e-lgpd.md)
- [Pendências Técnicas](docs/pendencias-tecnicas.md)
- [Roteiro de Apresentação](docs/roteiro-de-apresentacao.md)

Documentação interativa disponível via Swagger em `http://localhost:5297/swagger` após iniciar o backend.

---

## 👥 Equipe

- **Camila** — Desenvolvimento Backend
- **Kayque** — Banco de Dados
- **Mateus** — Desenvolvimento Frontend

TCC em Análise e Desenvolvimento de Sistemas
