// ui.js - Utilitários de interface

// Mostra mensagem de sucesso
function showSuccess(message) {
    showAlert(message, 'success');
}

// Mostra mensagem de erro
function showError(message) {
    showAlert(message, 'danger');
}

// Mostra alerta genérico
function showAlert(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.insertBefore(alertDiv, document.body.firstChild);
    setTimeout(() => alertDiv.remove(), 5000);
}

// Carrega opções em um select
function loadSelectOptions(selectElement, options, valueField = 'id', textField = 'name') {
    selectElement.innerHTML = '<option value="">Selecione...</option>';
    options.forEach(option => {
        const opt = document.createElement('option');
        opt.value = option[valueField];
        opt.textContent = option[textField];
        selectElement.appendChild(opt);
    });
}

// Formata data para dd/mm/yyyy
function formatDate(date) {
    const d = new Date(date);
    return d.toLocaleDateString('pt-BR');
}

// Formata hora para hh:mm
function formatTime(date) {
    const d = new Date(date);
    return d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
}

// Formata duração em minutos para hh:mm
function formatDuration(minutes) {
    const hours = Math.floor(minutes / 60);
    const mins = Math.floor(minutes % 60);
    return `${hours.toString().padStart(2, '0')}:${mins.toString().padStart(2, '0')}`;
}
