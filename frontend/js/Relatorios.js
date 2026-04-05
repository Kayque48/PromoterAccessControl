document.addEventListener('DOMContentLoaded', () => {
    carregarEmpresas();
    document.getElementById('btnGerar').addEventListener('click', gerarRelatorio);
});

async function carregarEmpresas() {
    try {
        const empresas = await apiRequest('/promotores/empresas');
        const filtro = document.getElementById('empresaFiltro');
        filtro.innerHTML = '<option value="">Todas</option>';
        empresas.forEach(emp => {
            filtro.innerHTML += `<option value="${emp.id}">${emp.nome}</option>`;
        });
    } catch (error) {
        console.error('Falha ao carregar empresas', error);
    }
}

async function gerarRelatorio() {
    const busca = document.getElementById('busca').value;
    const dataInicio = document.getElementById('dataInicio').value;
    const dataFim = document.getElementById('dataFim').value;
    const empresaId = document.getElementById('empresaFiltro').value;

    const params = new URLSearchParams();
    if (dataInicio) params.append('dataInicio', dataInicio);
    if (dataFim) params.append('dataFim', dataFim);
    if (empresaId) params.append('empresaId', empresaId);
    if (busca) params.append('busca', busca);

    try {
        const registros = await apiRequest(`/registros?${params.toString()}`);
        exibirResultado(registros);
    } catch (error) {
        alert('Erro ao gerar relatório: ' + error.message);
    }
}

let chartRelatorio = null;

function exibirResultado(registros) {
    const resultadoCard = document.getElementById('resultadoCard');
    resultadoCard.classList.remove('d-none');

    document.getElementById('totalRegistrosTxt').textContent = `Registros: ${registros.length}`;
    document.getElementById('registrosFiltradosTxt').textContent = `Filtrados: ${registros.length}`;

    const tbody = document.getElementById('tbodyHistorico');
    tbody.innerHTML = '';

    registros.forEach(r => {
        tbody.innerHTML += `
            <tr>
                <td>${r.promotor}</td>
                <td>${r.empresa}</td>
                <td>${r.data}</td>
                <td>${r.diaSemana}</td>
                <td>${r.entrada}</td>
                <td>${r.saida || '-'}</td>
                <td>${r.duracao || '-'}</td>
                <td>${r.diaCorreto ? '<span class="badge bg-success">Sim</span>' : '<span class="badge bg-danger">Não</span>'}</td>
                <td>${(r.diasPermitidos || []).join(', ')}</td>
            </tr>
        `;
    });

    const frequencia = registros.reduce((acc, cur) => {
        acc[cur.diaSemana] = (acc[cur.diaSemana] || 0) + 1;
        return acc;
    }, {});
    const labels = Object.keys(frequencia);
    const data = Object.values(frequencia);

    const ctx = document.getElementById('graficoRelatorio').getContext('2d');
    if (chartRelatorio) {
        chartRelatorio.destroy();
    }
    chartRelatorio = new Chart(ctx, {
        type: 'bar',
        data: {
            labels,
            datasets: [{
                label: 'Visitas por dia',
                data,
                backgroundColor: '#3b82f6'
            }]
        },
        options: {
            scales: {
                y: { beginAtZero: true }
            }
        }
    });
}