// exportar.js - Gerencia exportação de dados

document.addEventListener('DOMContentLoaded', async function() {
    requireAuth();
    atualizarRotulosIndicadores();
    
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

function atualizarRotulosIndicadores() {
    const mediaLabel = document.getElementById('mediaHoras')?.previousElementSibling;
    const promotoresLabel = document.getElementById('diasFrequentados')?.previousElementSibling;
    const empresasLabel = document.getElementById('consistencia')?.previousElementSibling;

    if (mediaLabel) mediaLabel.textContent = 'Duração média';
    if (promotoresLabel) promotoresLabel.textContent = 'Promotores únicos';
    if (empresasLabel) empresasLabel.textContent = 'Empresas únicas';
}

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
    const dataInicio = document.getElementById('dataInicio').value;
    const dataFim = document.getElementById('dataFim').value;
    const empresaId = document.getElementById('empresaFiltro').value;
    const promotorId = document.getElementById('promotorFiltro').value;
    const hoje = new Date();
    const inicioPadrao = new Date();
    inicioPadrao.setDate(hoje.getDate() - 30);

    return {
        dataInicio: dataInicio ? `${dataInicio}T00:00:00` : inicioPadrao.toISOString(),
        dataFim: dataFim ? `${dataFim}T23:59:59` : hoje.toISOString(),
        empresaId: empresaId ? parseInt(empresaId, 10) : null,
        promotorId: promotorId ? parseInt(promotorId, 10) : null
    };
}

function mostrarIndicadores(relatorio) {
    document.getElementById('totalRegistros').textContent = relatorio.totalRegistros || 0;
    document.getElementById('mediaHoras').textContent = formatarDuracaoHoras(relatorio.duracaoMediaMinutos || 0);
    document.getElementById('diasFrequentados').textContent = relatorio.promotoresUnicos || 0;
    document.getElementById('consistencia').textContent = relatorio.empresasUnicos || 0;
}

function renderizarRelatorio(relatorio) {
    const tbody = document.getElementById('tbodyRelatorio');
    if (!tbody) return;
    
    tbody.innerHTML = '';
    
    (relatorio.registros || []).forEach(reg => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${reg.promotorNome}</td>
            <td>${reg.empresaNome}</td>
            <td>${new Date(reg.entrada).toLocaleDateString('pt-BR')}</td>
            <td>${getDiaSemana(reg.entrada)}</td>
            <td>${new Date(reg.entrada).toLocaleTimeString('pt-BR')}</td>
            <td>${reg.saida ? new Date(reg.saida).toLocaleTimeString('pt-BR') : '-'}</td>
            <td>${formatarDuracaoMinutos(reg.duracaoMinutos)}</td>
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

function formatarDuracaoMinutos(minutos) {
    if (minutos === null || minutos === undefined) return '-';

    const horas = Math.floor(minutos / 60);
    const mins = minutos % 60;
    return `${horas}h ${mins}m`;
}

function formatarDuracaoHoras(minutos) {
    return `${(minutos / 60).toFixed(1)}h`;
}
