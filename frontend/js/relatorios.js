// relatorios.js - Lógica da página de relatórios

document.addEventListener('DOMContentLoaded', function() {
    loadEmpresasFiltro();
    document.getElementById('btnGerar').addEventListener('click', gerarRelatorio);
});

async function loadEmpresasFiltro() {
    try {
        const empresas = await getEmpresas();
        loadSelectOptions(document.getElementById('empresaFiltro'), empresas, 'id', 'name');
    } catch (error) {
        showError('Erro ao carregar empresas: ' + error.message);
    }
}

async function gerarRelatorio() {
    try {
        const [registros, promotores, empresas] = await Promise.all([
            getRegistrosAcesso(),
            getPromotores(),
            getEmpresas()
        ]);

        // Criar mapas para lookup rápido
        const promotoresMap = new Map(promotores.map(p => [p.id, p]));
        const empresasMap = new Map(empresas.map(e => [e.id, e]));

        // Aplicar filtros
        const busca = document.getElementById('busca').value.toLowerCase();
        const dataInicio = document.getElementById('dataInicio').value;
        const dataFim = document.getElementById('dataFim').value;
        const empresaFiltro = document.getElementById('empresaFiltro').value;

        let registrosFiltrados = registros.filter(registro => {
            const promotor = promotoresMap.get(registro.promoterId);
            if (!promotor) return false;

            const empresa = empresasMap.get(promotor.companyId);
            if (!empresa) return false;

            // Filtro de busca
            if (busca) {
                const matchNome = promotor.name.toLowerCase().includes(busca);
                const matchEmpresa = empresa.name.toLowerCase().includes(busca);
                if (!matchNome && !matchEmpresa) return false;
            }

            // Filtro de data
            if (dataInicio) {
                const dataEntrada = new Date(registro.entryTime);
                if (dataEntrada < new Date(dataInicio)) return false;
            }
            if (dataFim) {
                const dataEntrada = new Date(registro.entryTime);
                if (dataEntrada > new Date(dataFim + 'T23:59:59')) return false;
            }

            // Filtro de empresa
            if (empresaFiltro && promotor.companyId != empresaFiltro) return false;

            return true;
        });

        // Exibir resultados
        exibirRelatorio(registrosFiltrados, promotoresMap, empresasMap);

    } catch (error) {
        showError('Erro ao gerar relatório: ' + error.message);
    }
}

function exibirRelatorio(registros, promotoresMap, empresasMap) {
    const tbody = document.getElementById('tbodyHistorico');
    tbody.innerHTML = '';

    registros.forEach(registro => {
        const promotor = promotoresMap.get(registro.promoterId);
        const empresa = empresasMap.get(promotor.companyId);

        const dataEntrada = new Date(registro.entryTime);
        const dataSaida = registro.exitTime ? new Date(registro.exitTime) : null;

        const diaSemana = dataEntrada.toLocaleDateString('pt-BR', { weekday: 'long' });
        const entrada = formatTime(registro.entryTime);
        const saida = registro.exitTime ? formatTime(registro.exitTime) : '-';

        let duracao = '-';
        if (registro.exitTime) {
            const diff = (dataSaida - dataEntrada) / (1000 * 60); // minutos
            duracao = formatDuration(diff);
        }

        // Simulação de dia correto (ajuste conforme modelo)
        const diaCorreto = 'Sim'; // Placeholder

        // Dias permitidos (placeholder)
        const diasPermitidos = 'Seg, Ter, Qua'; // Placeholder

        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${promotor.name}</td>
            <td>${empresa.name}</td>
            <td>${formatDate(registro.entryTime)}</td>
            <td>${diaSemana}</td>
            <td>${entrada}</td>
            <td>${saida}</td>
            <td>${duracao}</td>
            <td>${diaCorreto}</td>
            <td>${diasPermitidos}</td>
        `;
        tbody.appendChild(row);
    });

    // Atualizar contadores
    document.getElementById('totalRegistrosTxt').textContent = `Registros: ${registros.length}`;
    document.getElementById('registrosFiltradosTxt').textContent = `Filtrados: ${registros.length}`;

    // Mostrar card de resultado
    document.getElementById('resultadoCard').classList.remove('d-none');

    // Atualizar gráfico (placeholder)
    atualizarGrafico(registros);
}

function atualizarGrafico(registros) {
    const ctx = document.getElementById('graficoRelatorio').getContext('2d');
    
    // Dados simples para o gráfico
    const dias = ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'];
    const contagens = dias.map(() => Math.floor(Math.random() * 10)); // Placeholder

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: dias,
            datasets: [{
                label: 'Registros por dia',
                data: contagens,
                backgroundColor: 'rgba(54, 162, 235, 0.5)',
                borderColor: 'rgba(54, 162, 235, 1)',
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}