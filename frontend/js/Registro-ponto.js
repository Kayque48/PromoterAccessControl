document.addEventListener('DOMContentLoaded', async () => {
    await carregarPromotores();
    await carregarRegistrosAbertos();

    document.getElementById('btnEntrada').addEventListener('click', registrarEntrada);
});

async function carregarRegistrosAbertos() {
    try {
        const abertos = await apiRequest('/registros/abertos');
        const container = document.getElementById('listaRegistrosAbertos');
        container.innerHTML = '';

        if (abertos.length === 0) {
            container.innerHTML = '<p class="text-muted">Nenhum promotor em atendimento no momento.</p>';
            return;
        }

        abertos.forEach(item => {
            const card = document.createElement('div');
            card.className = 'card mb-2';
            card.innerHTML = `
                <div class="card-body d-flex align-items-center justify-content-between">
                    <div>
                        <h6 class="mb-0">${item.promotor}</h6>
                        <small class="text-muted">${item.empresa}</small>
                    </div>
                    <div class="text-end">
                        <small class="text-muted">Entrada: ${new Date(item.entrada).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</small><br>
                        <button class="btn btn-sm btn-danger mt-2" onclick="registrarSaida(${item.id})">Registrar Saída</button>
                    </div>
                </div>
            `;
            container.appendChild(card);
        });
    } catch (error) {
        alert('Erro ao carregar registros abertos: ' + error.message);
    }
}

async function carregarPromotores() {
    try {
        const promotores = await apiRequest('/promotores');
        const select = document.getElementById('promotor');
        select.innerHTML = '<option value="">Selecione...</option>';
        promotores.filter(p => p.ativo).forEach(p => {
            select.innerHTML += `<option value="${p.id}">${p.nome} - ${p.empresa?.nome ?? ''}</option>`;
        });
    } catch (error) {
        alert('Erro ao carregar promotores: ' + error.message);
    }
}

function mostrarMensagem(texto, tipo) {
    const div = document.getElementById('mensagem');
    div.textContent = texto;
    div.className = `alert alert-${tipo}`;
    div.classList.remove('d-none');
    setTimeout(() => div.classList.add('d-none'), 5000);
}

async function registrarEntrada() {
    const promotorId = document.getElementById('promotor').value;
    if (!promotorId) {
        mostrarMensagem('Selecione um promotor.', 'danger');
        return;
    }

    try {
        await apiRequest('/registros/entrada', 'POST', parseInt(promotorId));
        mostrarMensagem('Entrada registrada com sucesso!', 'success');
        document.getElementById('promotor').value = '';
        await carregarRegistrosAbertos();
    } catch (error) {
        mostrarMensagem(error.message, 'danger');
    }
}

async function registrarSaida(registroId) {
    try {
        await apiRequest(`/registros/saida/${registroId}`, 'PUT');
        mostrarMensagem('Saída registrada com sucesso!', 'success');
        await carregarRegistrosAbertos();
    } catch (error) {
        mostrarMensagem(error.message, 'danger');
    }
}