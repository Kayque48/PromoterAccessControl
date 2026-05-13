// registro-ponto.js - Gerencia registro de ponto

let registrosAbertos = [];

document.addEventListener('DOMContentLoaded', async function() {
    // Verifica autenticação
    requireAuth();

    // Carrega promotores
    await carregarPromotoresSelect();

    // Carrega registros abertos
    await carregarRegistrosAbertos();

    // Listeners
    const btnEntrada = document.getElementById('btnEntrada');
    if (btnEntrada) {
        btnEntrada.addEventListener('click', handleRegistrarEntrada);
    }
});

async function carregarPromotoresSelect() {
    try {
        const promotores = await getPromotores();
        const select = document.getElementById('promotor');
        if (select) {
            promotores.forEach(prom => {
                const option = document.createElement('option');
                option.value = prom.id;
                option.textContent = prom.nome;
                select.appendChild(option);
            });
        }
    } catch (error) {
        console.error('Erro ao carregar promotores:', error);
    }
}

async function handleRegistrarEntrada() {
    const promoterSelect = document.getElementById('promotor');
    const promoterId = parseInt(promoterSelect.value, 10);

    if (Number.isNaN(promoterId)) {
        alert('Selecione um promotor');
        return;
    }

    try {
        // Obtém empresa do promotor
        const promoter = await getPromotor(promoterId);
        const empresaId = parseInt(promoter.empresaId, 10);

        if (Number.isNaN(empresaId)) {
            alert('Promotor sem empresa vinculada');
            return;
        }

        await registrarEntrada(promoterId, empresaId);

        alert('Entrada registrada com sucesso!');
        promoterSelect.value = '';

        // Recarrega registros
        await carregarRegistrosAbertos();
    } catch (error) {
        console.error('Erro ao registrar entrada:', error);
        alert('Erro ao registrar entrada');
    }
}

async function carregarRegistrosAbertos() {
    try {
        const registros = await getRegistrosAtivos();
        registrosAbertos = registros;
        renderizarRegistrosAbertos();
    } catch (error) {
        console.error('Erro ao carregar registros:', error);
    }
}

function renderizarRegistrosAbertos() {
    const container = document.getElementById('listaRegistrosAbertos');
    if (!container) return;

    if (registrosAbertos.length === 0) {
        container.innerHTML = '<p class="text-muted">Nenhum promotor em atendimento no momento.</p>';
        return;
    }

    container.innerHTML = '';

    const tabela = document.createElement('div');
    tabela.className = 'table-responsive';
    tabela.innerHTML = `
        <table class="table table-hover mb-0">
            <thead>
                <tr>
                    <th>Promotor</th>
                    <th>Entrada</th>
                    <th>Duração</th>
                    <th>Ação</th>
                </tr>
            </thead>
            <tbody>
                ${registrosAbertos.map(reg => `
                    <tr>
                        <td>${reg.promotorNome}</td>
                        <td>${new Date(reg.entradaEm).toLocaleTimeString('pt-BR')}</td>
                        <td>${calcularDuracao(reg.entradaEm)}</td>
                        <td>
                            <button class="btn btn-sm btn-primary" onclick="handleSaida(${reg.promotorId}, ${reg.empresaId})">Saída</button>
                        </td>
                    </tr>
                `).join('')}
            </tbody>
        </table>
    `;

    container.appendChild(tabela);
}

async function handleSaida(promotorId, empresaId) {
    if (!confirm('Registrar saída?')) return;

    try {
        await registrarSaida(promotorId, empresaId);
        alert('Saída registrada com sucesso!');
        await carregarRegistrosAbertos();
    } catch (error) {
        console.error('Erro ao registrar saída:', error);
        alert('Erro ao registrar saída');
    }
}

function calcularDuracao(entrada) {
    const agora = new Date();
    const entradaDate = new Date(entrada);
    const diff = agora - entradaDate;

    const horas = Math.floor(diff / 3600000);
    const minutos = Math.floor((diff % 3600000) / 60000);

    return `${horas}h ${minutos}m`;
}
