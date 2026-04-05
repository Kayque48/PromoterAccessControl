document.addEventListener('DOMContentLoaded', function () {
    // Login
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const login = document.getElementById('login').value;
            const senha = document.getElementById('senha').value;
            const erroDiv = document.getElementById('erro');

            try {
                const response = await fetch(`${API_BASE_URL}/auth/login`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ login, senha })
                });

                if (!response.ok) {
                    const err = await response.json();
                    throw new Error(err.erro || err.message || 'Usuário ou senha inválidos');
                }

                const data = await response.json();
                localStorage.setItem('token', data.token);
                // Simular role para acesso a páginas de gestor
                if (data.role) {
                    localStorage.setItem('role', data.role);
                } else {
                    // Caso não venha role do backend, default como gestor para dev
                    localStorage.setItem('role', 'Gestor');
                }
                window.location.href = 'dashboard.html';
            } catch (error) {
                erroDiv.textContent = error.message;
                erroDiv.classList.remove('d-none');
            }
        });
    }

    // Logout
    const btnLogout = document.getElementById('btnLogout');
    if (btnLogout) {
        btnLogout.addEventListener('click', () => {
            localStorage.removeItem('token');
            window.location.href = 'login.html';
        });
    }

    // Verificação de autenticação ativa para páginas protegidas
    const paginasProtegidas = ['dashboard.html', 'promotores.html', 'registro-ponto.html', 'relatorios.html', 'empresas.html'];
    const paginaAtual = window.location.pathname.split('/').pop();
    if (paginasProtegidas.includes(paginaAtual)) {
        const token = localStorage.getItem('token');
        if (!token) {
            window.location.href = 'login.html';
            return;
        }

        // caso o token exista, verifica papel mínimo para pagina empresas
        if (paginaAtual === 'empresas.html') {
            const role = localStorage.getItem('role');
            if (role !== 'Gestor' && role !== 'Admin') {
                alert('Acesso negado: somente Gestor ou Admin.');
                window.location.href = 'dashboard.html';
                return;
            }
        }
    }
});