// dashboard.js - Carrega dados do dashboard

document.addEventListener('DOMContentLoaded', async function() {
    // Verifica autenticação
    requireAuth();
    
    // Exibe nome do usuário
    const user = getCurrentUser();
    if (user.nome) {
        const badgeUser = document.querySelector('.badge-user');
        if (badgeUser) {
            badgeUser.textContent = `Bem-vindo, ${user.nome}`;
        }
    }
    
    // Carrega dados do dashboard
    await carregarDados();
});

async function carregarDados() {
    try {
        // Carrega dados do dashboard
        const dashboard = await getDashboardHoje();
        
        // Atualiza cards de indicadores
        atualizarIndicadores(dashboard);
        
        // Carrega gráficos
        await carregarGraficos();
        
    } catch (error) {
        console.error('Erro ao carregar dashboard:', error);
        mostrarErro('Erro ao carregar dados do dashboard');
    }
}

function atualizarIndicadores(dashboard) {
    document.getElementById('cardPromotoresAtivos').textContent = dashboard.totalPromotoresAtivos || 0;
    document.getElementById('cardVisitasHoje').textContent = dashboard.totalVisitasHoje || 0;
    document.getElementById('cardMediaHorasDia').textContent = (dashboard.mediaHorasPorPromotor || 0).toFixed(1) + 'h';
    document.getElementById('cardTotalRegistros').textContent = dashboard.totalRegistrosUltimos30Dias || 0;
}

async function carregarGraficos() {
    try {
        // Gráfico de frequência
        const visitasSemana = await getVisitasPorDiaSemana();
        if (visitasSemana && visitasSemana.length > 0) {
            criarGraficoFrequencia(visitasSemana);
        }
    } catch (error) {
        console.error('Erro ao carregar gráficos:', error);
    }
}

function criarGraficoFrequencia(dados) {
    const ctx = document.getElementById('graficoFrequencia');
    if (!ctx) return;
    
    const labels = dados.map(d => d.data);
    const values = dados.map(d => d.totalVisitas);
    
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Visitas',
                data: values,
                backgroundColor: 'rgba(92, 140, 255, 0.6)',
                borderColor: 'rgba(92, 140, 255, 1)',
                borderRadius: 8,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        color: 'rgba(255, 255, 255, 0.7)'
                    },
                    grid: {
                        color: 'rgba(255, 255, 255, 0.1)'
                    }
                },
                x: {
                    ticks: {
                        color: 'rgba(255, 255, 255, 0.7)'
                    },
                    grid: {
                        display: false
                    }
                }
            }
        }
    });
}

function mostrarErro(mensagem) {
    const alert = document.createElement('div');
    alert.className = 'alert alert-danger alert-dismissible fade show';
    alert.innerHTML = `
        ${mensagem}
        <button type=\"button\" class=\"btn-close\" data-bs-dismiss=\"alert\"></button>
    `;
    const main = document.querySelector('main');
    main.insertBefore(alert, main.firstChild);
}
