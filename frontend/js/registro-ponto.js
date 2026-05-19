// registro-ponto.js - Gerencia registro de ponto

let registrosAbertos = [];

function getArrayResponse(response) {
    if (Array.isArray(response)) return response;
    if (Array.isArray(response?.$values)) return response.$values;
    if (Array.isArray(response?.items)) return response.items;
    if (Array.isArray(response?.data)) return response.data;
    return [];
}

function getNumberValue(...values) {
    for (const value of values) {
        const numberValue = parseInt(value, 10);
        if (!Number.isNaN(numberValue)) return numberValue;
    }

    return null;
}

function getEmpresaIdPromotor(promotor) {
    const empresaIds = Array.isArray(promotor?.empresaIds)
        ? promotor.empresaIds
        : getArrayResponse(promotor?.empresaIds);

    return getNumberValue(
        promotor?.empresaId,
        promotor?.empresaExclusivaId,
        empresaIds[0]
    );
}

function normalizarRegistroAtivo(registro = {}) {
    const entradaEm = registro.entradaEm || registro.entrada || registro.dataHora || registro.EntryTime;
    const minutosEmAtendimento = getNumberValue(
        registro.minutosEmAtendimento,
        registro.minutosAtendimento,
        registro.duracaoMinutos,
        registro.permanenciaMin
    );

    return {
        promotorId: getNumberValue(registro.promotorId, registro.PromotorId, registro.promoterId),
        promotorNome: registro.promotorNome || registro.promotorName || registro.nomePromotor || registro.promotor?.nome || '-',
        empresaId: getNumberValue(registro.empresaId, registro.EmpresaId, registro.companyId),
        empresaNome: registro.empresaNome || registro.empresaName || registro.nomeEmpresa || registro.empresa?.razaoSocial || registro.empresa?.nomeFantasia || '-',
        entradaEm,
        minutosEmAtendimento
    };
}

function formatarHorarioEntrada(entradaEm) {
    const data = new Date(entradaEm);
    if (Number.isNaN(data.getTime())) return '-';
    return data.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
}

function formatarDuracaoMinutos(totalMinutos) {
    if (totalMinutos === null || Number.isNaN(totalMinutos)) return null;

    const horas = Math.floor(totalMinutos / 60);
    const minutos = totalMinutos % 60;
    return `${horas}h ${minutos}m`;
}

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
        const empresaId = getEmpresaIdPromotor(promoter);

        if (!empresaId) {
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
        registrosAbertos = getArrayResponse(registros).map(normalizarRegistroAtivo);
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
                ${registrosAbertos.map(reg => {
                    const podeRegistrarSaida = reg.promotorId && reg.empresaId;
                    const botaoSaida = podeRegistrarSaida
                        ? `<button class="btn btn-sm btn-primary" onclick="handleSaida(${reg.promotorId}, ${reg.empresaId})">Saída</button>`
                        : '<button class="btn btn-sm btn-secondary" disabled>Saída</button>';

                    return `
                        <tr>
                            <td>
                                <div>${reg.promotorNome}</div>
                                <small class="text-muted">${reg.empresaNome}</small>
                            </td>
                            <td>${formatarHorarioEntrada(reg.entradaEm)}</td>
                            <td>${calcularDuracao(reg.entradaEm, reg.minutosEmAtendimento)}</td>
                            <td>${botaoSaida}</td>
                        </tr>
                    `;
                }).join('')}
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

function calcularDuracao(entrada, minutosEmAtendimento = null) {
    const duracaoBackend = formatarDuracaoMinutos(minutosEmAtendimento);
    if (duracaoBackend) return duracaoBackend;

    const agora = new Date();
    const entradaDate = new Date(entrada);
    if (Number.isNaN(entradaDate.getTime())) return '-';

    const diff = agora - entradaDate;

    const horas = Math.floor(diff / 3600000);
    const minutos = Math.floor((diff % 3600000) / 60000);

    return `${horas}h ${minutos}m`;
}

window.handleSaida = handleSaida;