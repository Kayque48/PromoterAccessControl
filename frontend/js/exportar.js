// exportar.js - Gerencia exportação de dados

document.addEventListener('DOMContentLoaded', async function() {
    requireAuth();
    
    await carregarFiltros();
    
    const btnGerar = document.getElementById('btnGerarRelatorio');
    if (btnGerar) {
        btnGerar.addEventListener('click', gerarRelatorio);
    }
    
    const btnExportar = document.getElementById('btnExportarCSV');
    if (btnExportar) {
        btnExportar.addEventListener('click', exportarArquivo);
    }
    
    const btnResetar = document.getElementById('btnResetar');
    if (btnResetar) {
        btnResetar.addEventListener('click', resetarFiltros);
    }
});

async function carregarFiltros() {
    try {
        const empresas = await getEmpresas();
        const selectEmpresa = document.getElementById('empresaFiltro');
        if (selectEmpresa) {
            empresas.forEach(emp => {
                const option = document.createElement('option');
                option.value = emp.id;
                option.textContent = emp.nomeFantasia || emp.razaoSocial;
                selectEmpresa.appendChild(option);
            });
        }
        
        const promotores = await getPromotores();
        const selectPromotor = document.getElementById('promotorFiltro');
        if (selectPromotor) {
            promotores.forEach(prom => {
                const option = document.createElement('option');
                option.value = prom.id;
                option.textContent = prom.nome;
                selectPromotor.appendChild(option);
            });
        }
    } catch (error) {
        console.error('Erro ao carregar filtros:', error);
    }
}

async function gerarRelatorio() {
    try {
        const filtros = obterFiltros();
        const relatorio = await getRelatorio(filtros);
        
        mostrarIndicadores(relatorio);
        renderizarRelatorio(relatorio);
        
        document.getElementById('indicadoresRelatorio').style.display = 'grid';
        document.getElementById('graficosRelatorio').style.display = 'grid';
        document.getElementById('tabelaRelatorioCard').style.display = 'block';
    } catch (error) {
        console.error('Erro ao gerar relatório:', error);
        alert('Erro ao gerar relatório');
    }
}

function obterFiltros() {
    return {
        dataInicio: document.getElementById('dataInicio').value,
        dataFim: document.getElementById('dataFim').value,
        empresaId: document.getElementById('empresaFiltro').value || null,
        promotorId: document.getElementById('promotorFiltro').value || null
    };
}

function mostrarIndicadores(relatorio) {
    document.getElementById('totalRegistros').textContent = relatorio.totalRegistros || 0;
    document.getElementById('mediaHoras').textContent = (relatorio.mediaHoras || 0).toFixed(1) + 'h';
    document.getElementById('diasFrequentados').textContent = relatorio.diasFrequentados || 0;
    document.getElementById('consistencia').textContent = (relatorio.consistencia || 0).toFixed(0) + '%';
}

function renderizarRelatorio(relatorio) {
    const tbody = document.getElementById('tbodyRelatorio');
    if (!tbody || !relatorio.registros) return;
    
    tbody.innerHTML = '';
    
    relatorio.registros.forEach(reg => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${reg.promotorNome}</td>
            <td>${reg.empresaNome}</td>
            <td>${new Date(reg.data).toLocaleDateString('pt-BR')}</td>
            <td>${getDiaSemana(reg.data)}</td>
            <td>${new Date(reg.entrada).toLocaleTimeString('pt-BR')}</td>
            <td>${reg.saida ? new Date(reg.saida).toLocaleTimeString('pt-BR') : '-'}</td>
            <td>${reg.duracao || '-'}</td>
        `;
        tbody.appendChild(tr);
    });
}

async function exportarArquivo() {
    try {
        const filtros = obterFiltros();
        const blob = await exportarCSV(filtros);
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'relatorio.csv';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    } catch (error) {
        console.error('Erro ao exportar:', error);
        alert('Erro ao exportar CSV');
    }
}

function resetarFiltros() {
    document.getElementById('dataInicio').value = '';
    document.getElementById('dataFim').value = '';
    document.getElementById('empresaFiltro').value = '';
    document.getElementById('promotorFiltro').value = '';
    
    document.getElementById('indicadoresRelatorio').style.display = 'none';
    document.getElementById('graficosRelatorio').style.display = 'none';
    document.getElementById('tabelaRelatorioCard').style.display = 'none';
}

function getDiaSemana(data) {
    const dias = ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado'];
    return dias[new Date(data).getDay()];
}
