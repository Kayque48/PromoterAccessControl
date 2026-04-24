// auth.js - Funções de autenticação

const AUTH_API_BASE_URL = 'http://localhost:5297/api';

// Verifica se o usuário está logado
function isLoggedIn() {
    return localStorage.getItem('token') !== null;
}

// Redireciona para login se não estiver logado
function requireAuth() {
    if (!isLoggedIn()) {
        window.location.href = 'Login.html';
    }
}

// Logout
function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('usuarioId');
    localStorage.removeItem('usuarioNome');
    localStorage.removeItem('usuarioPerfil');
    window.location.href = 'Login.html';
}

// Login com API
async function login(login, senha) {
    try {
        const response = await fetch(`${AUTH_API_BASE_URL}/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ login, senha })
        });

        if (!response.ok) {
            let errorMessage = 'Falha na autenticação';
            try {
                const error = await response.json();
                errorMessage = error?.message || errorMessage;
            } catch {
                const text = await response.text();
                if (text) errorMessage = text;
            }
            throw new Error(errorMessage);
        }

        const data = await response.json();
        
        // Armazena token e dados do usuário
        if (data.token) {
            localStorage.setItem('token', data.token);
            localStorage.setItem('usuarioId', data.usuarioId);
            localStorage.setItem('usuarioNome', data.nome);
            localStorage.setItem('usuarioPerfil', data.perfil);
            return { success: true, user: data };
        }
        
        return { success: false, error: 'Token não recebido' };
    } catch (error) {
        console.error('Erro ao fazer login:', error);
        return { success: false, error: error.message };
    }
}

// Obtém token do localStorage
function getToken() {
    return localStorage.getItem('token');
}

// Obtém usuário logado
function getCurrentUser() {
    return {
        id: localStorage.getItem('usuarioId'),
        nome: localStorage.getItem('usuarioNome'),
        perfil: localStorage.getItem('usuarioPerfil')
    };
}

// Event listener para botão de logout
document.addEventListener('DOMContentLoaded', function() {
    const btnLogout = document.getElementById('btnLogout');
    if (btnLogout) {
        btnLogout.addEventListener('click', logout);
    }
});