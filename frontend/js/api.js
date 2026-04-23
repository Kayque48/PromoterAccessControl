// api.js - Funções para comunicação com a API

const API_BASE_URL = 'https://localhost:7272/api';

// Função para obter token do localStorage
function getAuthToken() {
    return localStorage.getItem('token');
}

// Função para adicionar token aos headers
function getAuthHeaders() {
    const token = getAuthToken();
    const headers = {
        'Content-Type': 'application/json'
    };
    
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    
    return headers;
}

// Função genérica para fazer requisições GET
async function apiGet(endpoint) {
    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method: 'GET',
            headers: getAuthHeaders()
        });
        
        if (response.status === 401) {
            // Token expirado ou inválido
            logout();
            throw new Error('Sessão expirada. Faça login novamente.');
        }
        
        if (!response.ok) {
            throw new Error(`Erro na requisição: ${response.status}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('Erro na requisição GET:', error);
        throw error;
    }
}

// Função genérica para fazer requisições POST
async function apiPost(endpoint, data) {
    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify(data)
        });
        
        if (response.status === 401) {
            logout();
            throw new Error('Sessão expirada. Faça login novamente.');
        }
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || `Erro na requisição: ${response.status}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('Erro na requisição POST:', error);
        throw error;
    }
}

// Função genérica para fazer requisições PUT
async function apiPut(endpoint, data) {
    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method: 'PUT',
            headers: getAuthHeaders(),
            body: JSON.stringify(data)
        });
        
        if (response.status === 401) {
            logout();
            throw new Error('Sessão expirada. Faça login novamente.');
        }
        
        if (!response.ok) {
            throw new Error(`Erro na requisição: ${response.status}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('Erro na requisição PUT:', error);
        throw error;
    }
}

// Função genérica para fazer requisições DELETE
async function apiDelete(endpoint) {
    try {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method: 'DELETE',
            headers: getAuthHeaders()
        });
        
        if (response.status === 401) {
            logout();
            throw new Error('Sessão expirada. Faça login novamente.');
        }
        
        if (!response.ok) {
            throw new Error(`Erro na requisição: ${response.status}`);
        }
        
        return response.status === 204 ? null : await response.json();
    } catch (error) {
        console.error('Erro na requisição DELETE:', error);
        throw error;
    }
}

// ========== FUNÇÕES ESPECÍFICAS ==========

// Dashboard
async function getDashboardHoje() {
    return await apiGet('/dashboard/hoje');
}

async function getVisitasPorDiaSemana() {
    return await apiGet('/dashboard/visitassemana');
}

// Promotores
async function getPromotores(empresaId = null) {
    const endpoint = empresaId ? `/promotores?empresaId=${empresaId}` : '/promotores';
    return await apiGet(endpoint);
}

async function getPromotor(id) {
    return await apiGet(`/promotores/${id}`);
}

async function createPromotor(data) {
    return await apiPost('/promotores', data);
}

async function updatePromotor(id, data) {
    return await apiPut(`/promotores/${id}`, data);
}

async function deletePromotor(id) {
    return await apiDelete(`/promotores/${id}`);
}

// Empresas
async function getEmpresas() {
    return await apiGet('/empresas');
}

async function getEmpresa(id) {
    return await apiGet(`/empresas/${id}`);
}

async function createEmpresa(data) {
    return await apiPost('/empresas', data);
}

async function updateEmpresa(id, data) {
    return await apiPut(`/empresas/${id}`, data);
}

async function deleteEmpresa(id) {
    return await apiDelete(`/empresas/${id}`);
}

// Registros de Acesso
async function getRegistrosAcesso() {
    return await apiGet('/registrosacessos');
}

async function registrarEntrada(promoterId, empresaId) {
    return await apiPost('/registrosacessos/entrada', { promoterId, empresaId });
}

async function registrarSaida(registroId) {
    return await apiPost(`/registrosacessos/${registroId}/saida`, {});
}

// Relatórios
async function getRelatorio(filtros) {
    const params = new URLSearchParams(filtros);
    return await apiGet(`/relatorios?${params.toString()}`);
}

async function exportarCSV(filtros) {
    const params = new URLSearchParams(filtros);
    return `${API_BASE_URL}/relatorios/exportar?${params.toString()}&Authorization=Bearer ${getAuthToken()}`;
}