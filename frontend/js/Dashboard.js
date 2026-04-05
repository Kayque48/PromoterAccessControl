document.addEventListener('DOMContentLoaded', async () => {
    try {
        const dados = await apiRequest('/dashboard');

        const frequenciaSemana = dados.frequenciaSemana || [];
        const promotoresPorEmpresa = dados.promotoresPorEmpresa || [];
        const tempoMedioPorEmpresa = dados.tempoMedioPorEmpresa || [];

        document.getElementById('cardPromotoresAtivos').innerText = dados.promotoresAtivos ?? 0;
        document.getElementById('cardVisitasHoje').innerText = dados.registrosHoje ?? 0;
        document.getElementById('cardTotalRegistros').innerText = dados.totalRegistros ?? 0;
        document.getElementById('cardMediaHorasDia').innerText = `${(dados.mediaHorasDia ?? 0).toFixed(1)}h`;

        new Chart(document.getElementById('graficoFrequencia'), {
            type: 'bar',
            data: {
                labels: frequenciaSemana.map(item => item.dia),
                datasets: [{
                    label: 'Visitas',
                    data: frequenciaSemana.map(item => item.total),
                    backgroundColor: '#3b82f6'
                }]
            },
            options: {
                scales: {
                    y: { beginAtZero: true }
                }
            }
        });

        new Chart(document.getElementById('graficoTempo'), {
            type: 'line',
            data: {
                labels: tempoMedioPorEmpresa.map(item => item.empresa),
                datasets: [{
                    label: 'Horas',
                    data: tempoMedioPorEmpresa.map(item => ((item.media || 0) / 60).toFixed(2)),
                    borderColor: '#8b5cf6',
                    backgroundColor: 'rgba(139,92,246,0.15)',
                    tension: 0.25,
                    fill: true
                }]
            },
            options: {
                scales: {
                    y: { beginAtZero: true }
                }
            }
        });

        new Chart(document.getElementById('graficoEmpresas'), {
            type: 'pie',
            data: {
                labels: promotoresPorEmpresa.map(item => item.empresa),
                datasets: [{
                    data: promotoresPorEmpresa.map(item => item.quantidade),
                    backgroundColor: ['#3b82f6', '#6366f1', '#f43f5e', '#f59e0b', '#10b981', '#8b5cf6']
                }]
            }
        });

        const rankingEl = document.getElementById('rankingEmpresas');
        rankingEl.innerHTML = '';
        promotoresPorEmpresa
            .sort((a, b) => b.quantidade - a.quantidade)
            .forEach((item, index) => {
                const li = document.createElement('li');
                li.className = 'rank-item';
                li.innerHTML = `<strong>${index + 1}. ${item.empresa}</strong> <span>${item.quantidade} visitas</span>`;
                rankingEl.appendChild(li);
            });
    } catch (error) {
        alert('Erro ao carregar dashboard: ' + error.message);
    }
});