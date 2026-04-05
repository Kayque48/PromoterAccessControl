# 🔗 Integração Frontend + Backend

## 📌 Visão Geral

O backend está preparado em `http://localhost:5000` e o frontend em `frontend/`.

Este guia mostra como conectar o JavaScript do frontend com a API.

---

## 🔧 Configuração Base

### **URL Base da API**
```javascript
const API_BASE_URL = 'http://localhost:5000/api';
let authToken = localStorage.getItem('authToken');
```

### **Função Helper para Requisições**
```javascript
async function apiCall(endpoint, method = 'GET', body = null) {
    const options = {
        method,
        headers: {
            'Content-Type': 'application/json'
        }
    };

    if (authToken) {
        options.headers['Authorization'] = `Bearer ${authToken}`;
    }

    if (body) {
        options.body = JSON.stringify(body);
    }

    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, options);
        
        if (response.status === 401) {
            // Token expirou
            localStorage.removeItem('authToken');
            window.location.href = '/Login.html';
            return null;
        }

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Erro na requisição');
        }

        return await response.json();
    } catch (error) {
        console.error('Erro:', error);
        alert('Erro: ' + error.message);
        return null;
    }
}
```

---

## 🔐 **1. Login (Login.html)**

### **Frontend → Backend**
```javascript
async function fazerLogin() {
    const login = document.getElementById('usuario').value;
    const senha = document.getElementById('senha').value;

    const response = await apiCall('/auth/login', 'POST', {
        login: login,
        senha: senha
    });

    if (response && response.token) {
        localStorage.setItem('authToken', response.token);
        localStorage.setItem('usuarioNome', response.nome);
        localStorage.setItem('usuarioPerfil', response.perfil);
        
        // Redirecionar para dashboard
        window.location.href = '/Dashboard.html';
    }
}
```

### **API Response**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "usuarioId": 1,
    "nome": "Administrador",
    "perfil": "admin"
}
```

---

## 🏢 **2. Empresas (Empresas.html)**

### **Listar Empresas**
```javascript
async function carregarEmpresas() {
    const empresas = await apiCall('/empresas');
    
    const tabela = document.getElementById('tabelaEmpresas');
    tabela.innerHTML = '';
    
    empresas.forEach(emp => {
        const row = `
            <tr>
                <td>${emp.id}</td>
                <td>${emp.razaoSocial}</td>
                <td>${emp.nomeFantasia}</td>
                <td>${emp.cnpj}</td>
                <td>${emp.email}</td>
                <td>
                    <button onclick="editarEmpresa(${emp.id})">Editar</button>
                    <button onclick="deletarEmpresa(${emp.id})">Deletar</button>
                </td>
            </tr>
        `;
        tabela.innerHTML += row;
    });
}
```

### **Criar Empresa**
```javascript
async function criarEmpresa() {
    const empresa = {
        cnpj: document.getElementById('cnpj').value,
        razaoSocial: document.getElementById('razaoSocial').value,
        nomeFantasia: document.getElementById('nomeFantasia').value,
        telefone: document.getElementById('telefone').value,
        email: document.getElementById('email').value,
        endereco: document.getElementById('endereco').value,
        numero: document.getElementById('numero').value,
        bairro: document.getElementById('bairro').value,
        cidade: document.getElementById('cidade').value,
        estado: document.getElementById('estado').value,
        cep: document.getElementById('cep').value
    };

    const resultado = await apiCall('/empresas', 'POST', empresa);
    
    if (resultado) {
        alert('Empresa criada com sucesso!');
        formulario.reset();
        carregarEmpresas();
    }
}
```

### **Atualizar Empresa**
```javascript
async function salvarEmpresa(id) {
    const empresa = {
        cnpj: document.getElementById('cnpj').value,
        razaoSocial: document.getElementById('razaoSocial').value,
        nomeFantasia: document.getElementById('nomeFantasia').value,
        telefone: document.getElementById('telefone').value,
        email: document.getElementById('email').value,
        // ... outros campos
    };

    const resultado = await apiCall(`/empresas/${id}`, 'PUT', empresa);
    
    if (resultado) {
        alert('Empresa atualizada!');
        carregarEmpresas();
    }
}
```

### **Deletar Empresa**
```javascript
async function deletarEmpresa(id) {
    if (confirm('Tem certeza que deseja deletar?')) {
        const resultado = await apiCall(`/empresas/${id}`, 'DELETE');
        
        if (resultado !== null) {
            alert('Empresa deletada!');
            carregarEmpresas();
        }
    }
}
```

---

## 👔 **3. Promotores (Promotores.html)**

### **Listar Promotores**
```javascript
async function carregarPromotores(empresaId = null) {
    let url = '/promotores';
    if (empresaId) {
        url += `?empresaId=${empresaId}`;
    }
    
    const promotores = await apiCall(url);
    
    const tabela = document.getElementById('tabelaPromotores');
    tabela.innerHTML = '';
    
    promotores.forEach(prom => {
        const row = `
            <tr>
                <td>${prom.id}</td>
                <td>${prom.nome}</td>
                <td>${prom.cpf}</td>
                <td>${prom.telefone}</td>
                <td>
                    <button onclick="editarPromotor(${prom.id})">Editar</button>
                    <button onclick="deletarPromotor(${prom.id})">Deletar</button>
                </td>
            </tr>
        `;
        tabela.innerHTML += row;
    });
}
```

### **Criar Promotor**
```javascript
async function criarPromotor() {
    const promotor = {
        nome: document.getElementById('nome').value,
        cpf: document.getElementById('cpf').value,
        telefone: document.getElementById('telefone').value,
        email: document.getElementById('email').value,
        endereco: document.getElementById('endereco').value,
        numero: document.getElementById('numero').value,
        bairro: document.getElementById('bairro').value,
        cidade: document.getElementById('cidade').value,
        estado: document.getElementById('estado').value,
        cep: document.getElementById('cep').value,
        empresaId: parseInt(document.getElementById('empresaId').value)
    };

    const resultado = await apiCall('/promotores', 'POST', promotor);
    
    if (resultado) {
        alert('Promotor criado!');
        formulario.reset();
        carregarPromotores();
    }
}
```

---

## ⏰ **4. Registro de Ponto (Registro-ponto.html)**

### **Registrar Entrada**
```javascript
async function registrarEntrada() {
    const promotorId = parseInt(document.getElementById('promotorId').value);
    
    const resultado = await apiCall('/registrosacesso/entrada', 'POST', {
        promotorId: promotorId
    });
    
    if (resultado) {
        alert(`${resultado.promotorNome} - Entrada registrada em ${resultado.entrada}`);
        carregarPromotoresAtivos();
    }
}
```

### **Registrar Saída**
```javascript
async function registrarSaida(registroId) {
    const resultado = await apiCall('/registrosacesso/saida', 'POST', {
        registroId: registroId
    });
    
    if (resultado) {
        alert(`Saída registrada! Tempo: ${resultado.tempoPermanenciaMinutos} minutos`);
        carregarPromotoresAtivos();
    }
}
```

### **Listar Promotores Ativos**
```javascript
async function carregarPromotoresAtivos() {
    const ativos = await apiCall('/registrosacesso/ativos');
    
    const tabela = document.getElementById('tabelaAtivos');
    tabela.innerHTML = '';
    
    ativos.forEach(prom => {
        const row = `
            <tr>
                <td>${prom.nome}</td>
                <td>${prom.entrada}</td>
                <td>${prom.minutosAtendimento} minutos</td>
                <td>
                    <button onclick="registrarSaida(${prom.id})">Registrar Saída</button>
                </td>
            </tr>
        `;
        tabela.innerHTML += row;
    });
}

// Atualizar a cada 30 segundos
setInterval(carregarPromotoresAtivos, 30000);
```

---

## 📊 **5. Dashboard (Dashboard.html)**

### **Carregar Dados do Dashboard**
```javascript
async function carregarDashboard() {
    const dashboard = await apiCall('/dashboard/hoje');
    
    if (dashboard) {
        document.getElementById('totalPromotores').textContent = dashboard.totalPromotoresAtivos;
        document.getElementById('totalVisitas').textContent = dashboard.totalVisitasHoje;
        document.getElementById('mediaHoras').textContent = dashboard.mediaHorasPorPromotor.toFixed(2) + 'h';
        document.getElementById('totalRegistros30').textContent = dashboard.totalRegistrosUltimos30Dias;
    }
}
```

### **Gráfico: Visitas por Dia**
```javascript
async function carregarGraficoVisitas() {
    const dados = await apiCall('/dashboard/visitassemana');
    
    const labels = dados.map(d => new Date(d.data).toLocaleDateString('pt-BR'));
    const values = dados.map(d => d.totalVisitas);
    
    // Usar Chart.js
    const ctx = document.getElementById('graficoVisitas').getContext('2d');
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Visitas por Dia',
                data: values,
                backgroundColor: '#4CAF50'
            }]
        }
    });
}
```

### **Gráfico: Duração Média por Promotor**
```javascript
async function carregarGraficoDuracao() {
    const dados = await apiCall('/dashboard/duraomedia?limitePromotores=10');
    
    const labels = dados.map(d => d.nomePromotor);
    const values = dados.map(d => d.duracaoMediaMinutos / 60); // converter para horas
    
    const ctx = document.getElementById('graficoDuracao').getContext('2d');
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Duração Média (horas)',
                data: values,
                borderColor: '#2196F3',
                fill: false
            }]
        }
    });
}
```

---

## 📄 **6. Relatórios (Relatorios.html)**

### **Gerar Relatório**
```javascript
async function gerarRelatorio() {
    const dataInicio = document.getElementById('dataInicio').value;
    const dataFim = document.getElementById('dataFim').value;
    const empresaId = document.getElementById('empresaId').value;
    const promotorId = document.getElementById('promotorId').value;
    
    const relatorio = await apiCall('/relatorios/agregado', 'POST', {
        dataInicio: new Date(dataInicio).toISOString(),
        dataFim: new Date(dataFim).toISOString(),
        empresaId: empresaId ? parseInt(empresaId) : null,
        promotorId: promotorId ? parseInt(promotorId) : null
    });
    
    if (relatorio) {
        exibirRelatorio(relatorio);
    }
}

function exibirRelatorio(relatorio) {
    document.getElementById('totalRegistros').textContent = relatorio.totalRegistros;
    document.getElementById('duracaoMedia').textContent = relatorio.duracaoMediaMinutos.toFixed(2) + ' min';
    document.getElementById('promotoresUnicos').textContent = relatorio.promotoresUnicos;
    document.getElementById('empresasUnicos').textContent = relatorio.empresasUnicos;
    
    // Tabela com registros
    const tabela = document.getElementById('tabelaRegistros');
    tabela.innerHTML = '';
    relatorio.registros.forEach(reg => {
        const row = `
            <tr>
                <td>${reg.promotorNome}</td>
                <td>${reg.empresaNome}</td>
                <td>${new Date(reg.entrada).toLocaleString('pt-BR')}</td>
                <td>${reg.saida ? new Date(reg.saida).toLocaleString('pt-BR') : '-'}</td>
                <td>${reg.duracaoMinutos ? reg.duracaoMinutos + ' min' : '-'}</td>
            </tr>
        `;
        tabela.innerHTML += row;
    });
}
```

### **Exportar CSV**
```javascript
async function exportarCSV() {
    const dataInicio = document.getElementById('dataInicio').value;
    const dataFim = document.getElementById('dataFim').value;
    
    const response = await fetch(`${API_BASE_URL}/relatorios/exportar-csv`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${authToken}`
        },
        body: JSON.stringify({
            dataInicio: new Date(dataInicio).toISOString(),
            dataFim: new Date(dataFim).toISOString(),
            empresaId: null,
            promotorId: null
        })
    });
    
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `relatorio_${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
}
```

---

## 🔄 **CORS - Configuração Frontend**

### **Problemas Potenciais**
Se receber erro de CORS, confirme que `Program.cs` tem:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

---

## 🧪 **Testes com REST Client**

No VS Code, abra `test-api.http` e copie as requisições. Configure o token após fazer login.

---

## 📍 **Referência Rápida de Endpoints**

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/auth/login` | Login |
| GET | `/api/empresas` | Listar empresas |
| POST | `/api/empresas` | Criar empresa |
| GET | `/api/promotores` | Listar promotores |
| POST | `/api/promotores` | Criar promotor |
| POST | `/api/registrosacesso/entrada` | Registrar entrada |
| POST | `/api/registrosacesso/saida` | Registrar saída |
| GET | `/api/registrosacesso/ativos` | Promotores ativos |
| GET | `/api/dashboard/hoje` | Dashboard |
| POST | `/api/relatorios/agregado` | Gerar relatório |
| POST | `/api/relatorios/exportar-csv` | Exportar CSV |

---

## ✅ Checklist Integração

- [ ] Backend rodando em `http://localhost:5000`
- [ ] CORS configurado corretamente
- [ ] `API_BASE_URL` definida no frontend
- [ ] Função `apiCall()` implementada
- [ ] Login funcionando e token sendo salvo
- [ ] Empresas sendo listadas e criadas
- [ ] Promotores sendo gerenciados
- [ ] Entrada/Saída registrando corretamente
- [ ] Dashboard carregando dados
- [ ] Relatório sendo gerado
- [ ] CSV sendo baixado

---

## 🐛 Troubleshooting

### **Erro de CORS**
```
Access to XMLHttpRequest at 'http://localhost:5000/api/...' from origin 'file://...'
```
**Solução:** Rodando via arquivo local. Use um servidor HTTP:
```bash
cd frontend
python -m http.server 8000
# Acessar em http://localhost:8000
```

### **Erro 401 Unauthorized**
```
Token não está sendo enviado no header
```
**Solução:** Confirme que `authToken` está no localStorage:
```javascript
console.log(localStorage.getItem('authToken'));
```

### **Erro 404 Not Found**
```
Endpoint não encontrado
```
**Solução:** Verifique a grafia da rota e confirme que o backend está rodando.

---

📚 **Para detalhes completos, veja [API_DOCUMENTATION.md](./API_DOCUMENTATION.md)**
