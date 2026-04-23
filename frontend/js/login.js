// login.js - Gerencia login

document.addEventListener('DOMContentLoaded', function() {
    const loginForm = document.querySelector('form');
    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }
});

async function handleLogin(e) {
    e.preventDefault();
    
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    
    try {
        const resultado = await login(username, password);
        
        if (resultado.success) {\n            // Redireciona para dashboard\n            window.location.href = 'Dashboard.html';
        } else {
            mostrarErro(resultado.error || 'Erro ao fazer login');
        }
    } catch (error) {
        console.error('Erro:', error);
        mostrarErro('Erro ao fazer login. Tente novamente.');
    }
}

function mostrarErro(mensagem) {
    const mensagemDiv = document.getElementById('mensagem') || document.querySelector('.card-body');
    if (mensagemDiv) {
        const alert = document.createElement('div');
        alert.className = 'alert alert-danger alert-dismissible fade show';
        alert.innerHTML = `
            ${mensagem}
            <button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"alert\"></button>
        `;
        mensagemDiv.appendChild(alert);
    }
}
