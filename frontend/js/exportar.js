let chartFrequencia = null;
let chartTempo = null;
let ultimoRegistroNome = null;

document.addEventListener('DOMContentLoaded', () => {
    carregarEmpresas();
    document.getElementById('empresaFiltro').addEventListener('change', carregarPromotores);
    document.getElementById('btnGerarRelatorio').addEventListener('click', gerarRelatorio);
    document.getElementById('btnExportarCSV').addEventListener('click', exportarCSV);
});

async function carregarEmpresas() {
    try {
        const empresas = await apiRequest('/promotores/empresas');
        const filtro = document.getElementById('empresaFiltro');
        filtro.innerHTML = '<option value="">Todas</option>';
        empresas.forEach(emp => {
            filtro.innerHTML += `<option value="${emp.id}">${emp.nome}</option>`;
        });
        await carregarPromotores();
    } catch (error) {
        mostrarMensagem('Erro ao carregar empresas: ' + error.message, 'danger');
    }
}

async function carregarPromotores() {
    try {
        const empresaId = document.getElementById('empresaFiltro').value;
        const promotores = await apiRequest(`/promotores${empresaId ? '?empresaId=' + empresaId : ''}`);
        const filtro = document.getElementById('promotorFiltro');
        filtro.innerHTML = '<option value="">Todos</option>';
        promotores.forEach(p => {
            filtro.innerHTML += `<option value="${p.id}">${p.nome}</option>`;
        });
    } catch (error) {
        mostrarMensagem('Erro ao carregar promotores: ' + error.message, 'danger');
    }
}

function mostrarMensagem(texto, tipo) {
    const div = document.getElementById('mensagemExportar');
    div.textContent = texto;
    div.className = `alert alert-${tipo}`;
    setTimeout(() => { div.textContent = ''; div.className = ''; }, 5000);
}

async function gerarRelatorio() {
    const dataInicio = document.getElementById('dataInicio').value;
    const dataFim = document.getElementById('dataFim').value;
    const empresaId = document.getElementById('empresaFiltro').value;
    const promotorId = document.getElementById('promotorFiltro').value;

    const params = new URLSearchParams();
    if (dataInicio) params.append('dataInicio', dataInicio);
    if (dataFim) params.append('dataFim', dataFim);
    if (empresaId) params.append('empresaId', empresaId);
    if (promotorId) params.append('promotorId', promotorId);

    try {
        const registros = await apiRequest(`/registros?${params.toString()}`);
        if (!Array.isArray(registros) || registros.length === 0) {
            mostrarMensagem('Nenhum registro encontrado para o filtro.', 'warning');
            document.getElementById('indicadoresRelatorio').style.display = 'none';
            document.getElementById('graficosRelatorio').style.display = 'none';
            document.getElementById('tabelaRelatorioCard').style.display = 'none';
            return;
        }

        montarIndicadores(registros);
        montarTabela(registros);
        montarGraficos(registros);
        document.getElementById('indicadoresRelatorio').style.display = 'flex';
        document.getElementById('graficosRelatorio').style.display = 'block';
        document.getElementById('tabelaRelatorioCard').style.display = 'block';
        ultimoRegistroNome = `${dataInicio || 'inicio'}_${dataFim || 'fim'}_${empresaId || 'all'}_${promotorId || 'all'}`;

        if (promotorId) {
            const select = document.getElementById('promotorFiltro');
            const option = select.options[select.selectedIndex];
            const promotor = option ? option.text : 'Todos';
            gerarAnalisedeFrequencia(registros, promotor);
        } else {
            gerarAnalisedeFrequencia(registros, 'Todos');
        }

    } catch (error) {
        mostrarMensagem('Erro ao gerar relatório: ' + error.message, 'danger');
    }
}

function montarIndicadores(registros) {
    const total = registros.length;
    const mediaHoras = registros.reduce((soma, r) => {
        const dur = r.duracao ? parseDuracao(r.duracao) : 0;
        return soma + dur;
    }, 0) / total;

    const diasDistintos = new Set(registros.map(r => r.data)).size;
    const diasPossiveis = taxaDiasUteis(document.getElementById('dataInicio').value, document.getElementById('dataFim').value);
    const consistencia = diasPossiveis > 0 ? Math.round((diasDistintos / diasPossiveis) * 100) : 0;

    document.getElementById('totalRegistros').textContent = total;
    document.getElementById('mediaHoras').textContent = `${mediaHoras.toFixed(1)}h`;
    document.getElementById('diasFrequentados').textContent = diasDistintos;
    document.getElementById('consistencia').textContent = `${consistencia}%`;
}

function parseDuracao(duracao) {
    const partes = duracao.match(/(\d+)h\s*(\d+)?/);
    if (!partes) return 0;
    const horas = parseInt(partes[1], 10);
    const minutos = partes[2] ? parseInt(partes[2], 10) : 0;
    return horas + minutos / 60;
}

function taxaDiasUteis(dataInicio, dataFim) {
    const i = new Date(dataInicio);
    const f = new Date(dataFim);
    if (isNaN(i) || isNaN(f) || i > f) return 0;
    let dias = 0;
    for (let d = new Date(i); d <= f; d.setDate(d.getDate() + 1)) {
        const n = d.getDay();
        if (n !== 0 && n !== 6) dias++;
    }
    return dias;
}

function montarTabela(registros) {
    const tbody = document.getElementById('tbodyRelatorio');
    tbody.innerHTML = '';
    registros.forEach(r => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${r.promotor}</td>
            <td>${r.empresa}</td>
            <td>${r.data}</td>
            <td>${r.diaSemana}</td>
            <td>${r.entrada}</td>
            <td>${r.saida || '-'}</td>
            <td>${r.duracao || '-'}</td>
        `;
        tbody.appendChild(tr);
    });
}

function montarGraficos(registros) {
    const frequenciaPorDia = {}; 
    const tempoPorPromotor = {}; 
    registros.forEach(r => {
        frequenciaPorDia[r.diaSemana] = (frequenciaPorDia[r.diaSemana] || 0) + 1;
        const dur = r.duracao ? parseDuracao(r.duracao) : 0;
        if (!tempoPorPromotor[r.promotor]) tempoPorPromotor[r.promotor] = { soma: 0, cont: 0 };
        tempoPorPromotor[r.promotor].soma += dur;
        tempoPorPromotor[r.promotor].cont += 1;
    });

    const labelsFreq = ['Domingo','Segunda','Terça','Quarta','Quinta','Sexta','Sábado'];
    const dataFreq = labelsFreq.map(d => frequenciaPorDia[d] || 0);

    const ctxFreq = document.getElementById('graficoFrequencia').getContext('2d');
    if (chartFrequencia) chartFrequencia.destroy();
    chartFrequencia = new Chart(ctxFreq, {
        type: 'bar',
        data: { labels: labelsFreq, datasets: [{ label: 'Visitas', data: dataFreq, backgroundColor: '#3b82f6' }] },
        options: { scales: { y: { beginAtZero: true } } }
    });

    const labelsTempo = Object.keys(tempoPorPromotor);
    const dataTempo = labelsTempo.map(k => (tempoPorPromotor[k].soma / Math.max(1, tempoPorPromotor[k].cont)).toFixed(2));
    const ctxTempo = document.getElementById('graficoTempo').getContext('2d');
    if (chartTempo) chartTempo.destroy();
    chartTempo = new Chart(ctxTempo, {
        type: 'line',
        data: { labels: labelsTempo, datasets: [{ label: 'Horas médias', data: dataTempo, borderColor: '#8b5cf6', backgroundColor: 'rgba(139,92,246,0.1)', fill: true, tension: 0.3 }] },
        options: { scales: { y: { beginAtZero: true } } }
    });
}

function gerarAnalisedeFrequencia(registros, promotorNome) {
    const mapa = {};
    registros.forEach(r => {
        if (!mapa[r.diaSemana]) mapa[r.diaSemana] = 0;
        mapa[r.diaSemana]++;
    });
    let regras = 'Frequência por dia: ';
    for (const k of ['Segunda','Terça','Quarta','Quinta','Sexta']) {
        regras += `${k}: ${mapa[k] || 0}; `;
    }
    mostrarMensagem(`Relatório para ${promotorNome}. ${regras}`, 'success');
}

async function exportarCSV() {
    if (!document.getElementById('tabelaRelatorioCard').style.display || document.getElementById('tabelaRelatorioCard').style.display === 'none') {
        mostrarMensagem('Gere o relatório antes de exportar.', 'warning');
        return;
    }

    const dataInicio = document.getElementById('dataInicio').value;
    const dataFim = document.getElementById('dataFim').value;
    const empresaId = document.getElementById('empresaFiltro').value;
    const promotorId = document.getElementById('promotorFiltro').value;

    const params = new URLSearchParams();
    if (dataInicio) params.append('dataInicio', dataInicio);
    if (dataFim) params.append('dataFim', dataFim);
    if (empresaId) params.append('empresaId', empresaId);
    if (promotorId) params.append('promotorId', promotorId);

    try {
        const registros = await apiRequest(`/registros?${params.toString()}`);
        if (!Array.isArray(registros) || registros.length === 0) {
            mostrarMensagem('Nenhum registro para exportar.', 'warning');
            return;
        }

        const headers = ['Promotor', 'Empresa', 'Data', 'DiaSemana', 'Entrada', 'Saida', 'Duracao'];
        const linhas = [headers.join(',')];
        registros.forEach(r => {
            const row = [r.promotor, r.empresa, r.data, r.diaSemana, r.entrada, r.saida || '', r.duracao || ''];
            linhas.push(row.map(v => `"${String(v).replace(/"/g, '""')}"`).join(','));
        });

        const csv = linhas.join('\n');
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = `relatorio-e-exportar-${dataInicio || 'inicio'}-${dataFim || 'fim'}.csv`;
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        mostrarMensagem('CSV exportado com sucesso.', 'success');
    } catch (error) {
        mostrarMensagem('Erro ao exportar CSV: ' + error.message, 'danger');
    }
}
