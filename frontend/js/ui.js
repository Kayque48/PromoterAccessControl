function showToast(message, type = 'info') {
    let toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toastContainer';
        toastContainer.style.position = 'fixed';
        toastContainer.style.top = '1rem';
        toastContainer.style.right = '1rem';
        toastContainer.style.zIndex = '1055';
        document.body.appendChild(toastContainer);
    }

    const toastEl = document.createElement('div');
    toastEl.className = `toast align-items-center text-white bg-${type} border-0`;
    toastEl.style.minWidth = '220px';
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');

    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Fechar"></button>
        </div>
    `;

    toastContainer.appendChild(toastEl);
    const toast = new bootstrap.Toast(toastEl, { delay: 3000 });
    toast.show();

    toastEl.addEventListener('hidden.bs.toast', () => {
        toastEl.remove();
    });
}

function setTheme(theme) {
    document.body.classList.remove('light', 'dark');
    document.body.classList.add(theme);
    localStorage.setItem('theme', theme);
}

function initTheme() {
    const saved = localStorage.getItem('theme') || 'light';
    setTheme(saved);

    const toggle = document.getElementById('themeToggle');
    if (toggle) {
        toggle.textContent = saved === 'dark' ? 'Modo Claro' : 'Modo Escuro';
        toggle.onclick = () => {
            const next = document.body.classList.contains('dark') ? 'light' : 'dark';
            setTheme(next);
            toggle.textContent = next === 'dark' ? 'Modo Claro' : 'Modo Escuro';
            showToast(`Tema alterado para ${next === 'dark' ? 'Escuro' : 'Claro'}`, 'info');
        };
    }
}

function setRoleMenu() {
    const role = localStorage.getItem('role');
    const empresaLink = document.querySelector('.nav-link[href="empresas.html"]');
    if (empresaLink) {
        empresaLink.style.display = (role === 'Gestor' || role === 'Admin') ? '' : 'none';
    }
}

function checkEmpresaAccess() {
    const role = localStorage.getItem('role');
    if (role !== 'Gestor' && role !== 'Admin') {
        showToast('Acesso negado: somente gestores e administradores', 'danger');
        window.location.href = 'dashboard.html';
    }
}

window.addEventListener('DOMContentLoaded', () => {
    initTheme();
    setRoleMenu();
});