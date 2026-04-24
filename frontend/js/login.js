// login.js - Gerencia login

document.addEventListener('DOMContentLoaded', function() {
    if (isLoggedIn()) {
        window.location.href = 'Dashboard.html';
        return;
    }

    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }
});

async function handleLogin(e) {
    e.preventDefault();
    
    const username = document.getElementById('username').value.trim();
    const password = document.getElementById('password').value.trim();
    
    if (!username || !password) {
        mostrarErro('Preencha usuário e senha.');
        return;
    }

    try {
        const resultado = await login(username, password);
        
        if (resultado.success) {
            window.location.href = 'Dashboard.html';
        } else {
            mostrarErro(resultado.error || 'Erro ao fazer login');
        }
    } catch (error) {
        console.error('Erro:', error);
        mostrarErro('Erro ao fazer login. Tente novamente.');
    }
}

function mostrarErro(mensagem) {
    const mensagemDiv = document.getElementById('mensagem');
    if (!mensagemDiv) return;

    mensagemDiv.innerHTML = '';
    const alert = document.createElement('div');
    alert.className = 'alert alert-danger alert-dismissible fade show';
    alert.innerHTML = `
        ${mensagem}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    mensagemDiv.appendChild(alert);
}
