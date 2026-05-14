// relatorios.js - Logica da pagina de relatorios

let graficoRelatorioAtual = null;

document.addEventListener('DOMContentLoaded', function() {
    requireAuth();
    carregarEmpresasFiltro();

    document.getElementById('btnGerar')?.addEventListener('click', gerarRelatorio);
    document.getElementById('btnAtualizar')?.addEventListener('click', gerarRelatorio);
});

async function carregarEmpresasFiltro() {
    try {
        const empresas = await getEmpresas();
        const select = document.getElementById('empresaFiltro');
        if (!select) return;

        select.innerHTML = '<option value="">Todas</option>';
        empresas.forEach(empresa => {
            const option = document.createElement('option');
            option.value = empresa.id;
            option.textContent = empresa.nomeFantasia || empresa.razaoSocial;
            select.appendChild(option);
        });
    } catch (error) {
        mostrarErro('Erro ao carregar empresas: ' + error.message);
    }
}

function obterFiltrosRelatorio() {
    const dataInicio = document.getElementById('dataInicio').value;
    const dataFim = document.getElementById('dataFim').value;
    const empresaId = document.getElementById('empresaFiltro').value;

    const hoje = new Date();
    const inicioPadrao = new Date();
    inicioPadrao.setDate(hoje.getDate() - 30);

    return {
        dataInicio: dataInicio ? `${dataInicio}T00:00:00` : inicioPadrao.toISOString(),
        dataFim: dataFim ? `${dataFim}T23:59:59` : hoje.toISOString(),
        empresaId: empresaId ? parseInt(empresaId, 10) : null,
        promotorId: null
    };
}

async function gerarRelatorio() {
    try {
        const relatorio = await getRelatorio(obterFiltrosRelatorio());
        const busca = document.getElementById('busca').value.trim().toLowerCase();
        const registros = filtrarRegistrosPorBusca(relatorio.registros || [], busca);

        exibirRelatorio({
            ...relatorio,
            registros
        });
    } catch (error) {
        console.error('Erro ao gerar relatorio:', error);
        mostrarErro('Erro ao gerar relatorio: ' + error.message);
    }
}

function filtrarRegistrosPorBusca(registros, busca) {
    if (!busca) return registros;

    return registros.filter(registro => {
        const promotor = (registro.promotorNome || '').toLowerCase();
        const empresa = (registro.empresaNome || '').toLowerCase();
        return promotor.includes(busca) || empresa.includes(busca);
    });
}

function exibirRelatorio(relatorio) {
    const tbody = document.getElementById('tbodyHistorico');
    if (!tbody) return;

    const registros = relatorio.registros || [];
    tbody.innerHTML = '';

    registros.forEach(registro => {
        const entrada = new Date(registro.entrada);
        const row = document.createElement('tr');

        row.innerHTML = `
            <td>${registro.promotorNome}</td>
            <td>${registro.empresaNome}</td>
            <td>${formatarData(registro.entrada)}</td>
            <td>${entrada.toLocaleDateString('pt-BR', { weekday: 'long' })}</td>
            <td>${formatarHora(registro.entrada)}</td>
            <td>${registro.saida ? formatarHora(registro.saida) : '-'}</td>
            <td>${formatarDuracao(registro.duracaoMinutos)}</td>
            <td>-</td>
            <td>-</td>
        `;

        tbody.appendChild(row);
    });

    document.getElementById('totalRegistrosTxt').textContent = `Registros: ${relatorio.totalRegistros || 0}`;
    document.getElementById('registrosFiltradosTxt').textContent = `Filtrados: ${registros.length}`;
    document.getElementById('resultadoCard').classList.remove('d-none');

    atualizarGrafico(registros);
}

function atualizarGrafico(registros) {
    const canvas = document.getElementById('graficoRelatorio');
    if (!canvas || typeof Chart === 'undefined') return;

    const dias = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sab'];
    const contagens = Array(7).fill(0);

    registros.forEach(registro => {
        const dia = new Date(registro.entrada).getDay();
        contagens[dia] += 1;
    });

    if (graficoRelatorioAtual) {
        graficoRelatorioAtual.destroy();
    }

    graficoRelatorioAtual = new Chart(canvas.getContext('2d'), {
        type: 'bar',
        data: {
            labels: dias,
            datasets: [{
                label: 'Registros por dia',
                data: contagens,
                backgroundColor: 'rgba(92, 140, 255, 0.35)',
                borderColor: 'rgba(92, 140, 255, 0.9)',
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                }
            }
        }
    });
}

function formatarData(data) {
    return new Date(data).toLocaleDateString('pt-BR');
}

function formatarHora(data) {
    return new Date(data).toLocaleTimeString('pt-BR', {
        hour: '2-digit',
        minute: '2-digit'
    });
}

function formatarDuracao(minutos) {
    if (minutos === null || minutos === undefined) return '-';

    const horas = Math.floor(minutos / 60);
    const mins = minutos % 60;
    return `${horas}h ${mins}m`;
}

function mostrarErro(mensagem) {
    alert(mensagem);
}
