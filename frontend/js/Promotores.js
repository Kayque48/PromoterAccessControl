let empresas = [];

document.addEventListener('DOMContentLoaded', async () => {
    await carregarEmpresas();
    await carregarPromotores();

    document.getElementById('formPromotor').addEventListener('submit', salvarPromotor);

    const modalPromotorEl = document.getElementById('modalPromotor');
    window.modalPromotor = new bootstrap.Modal(modalPromotorEl);

    document.getElementById('btnNovoPromotor').addEventListener('click', () => {
        limparForm();
        document.getElementById('modalPromotorLabel').textContent = 'Novo Promotor';
        document.getElementById('btnSalvar').textContent = 'Cadastrar';
        modalPromotor.show();
    });

    const btnBuscarCep = document.getElementById('btnBuscarCep');
    if (btnBuscarCep) {
        btnBuscarCep.addEventListener('click', buscarCep);
    }

    const cepInput = document.getElementById('cep');
    if (cepInput) {
        cepInput.addEventListener('blur', buscarCep);
        cepInput.addEventListener('input', (e) => e.target.value = formatarCEP(e.target.value));
    }

    const cpfInput = document.getElementById('cpf');
    if (cpfInput) {
        cpfInput.addEventListener('input', (e) => e.target.value = formatarCPF(e.target.value));
    }

    const telefoneInput = document.getElementById('telefone');
    if (telefoneInput) {
        telefoneInput.addEventListener('input', (e) => e.target.value = formatarTelefone(e.target.value));
    }
});

async function carregarEmpresas() {
    try {
        const empresasResp = await apiRequest('/promotores/empresas');
        empresas = empresasResp;
        localStorage.setItem('empresasUpdated', Date.now());
        const select = document.getElementById('empresa');
        if (select) {
            select.innerHTML = '<option value="">Selecione...</option>';
            empresas.forEach(emp => {
                select.innerHTML += `<option value="${emp.id}">${emp.razaoSocial || emp.nome}</option>`;
            });
        }

        const filtroEmpresa = document.getElementById('empresaFiltro');
        if (filtroEmpresa) {
            filtroEmpresa.innerHTML = '<option value="">Todas</option>';
            empresas.forEach(emp => {
                filtroEmpresa.innerHTML += `<option value="${emp.id}">${emp.nome}</option>`;
            });
        }
    } catch (error) {
        console.error('Erro ao carregar empresas:', error);
    }
}

async function carregarPromotores() {
    try {
        const promotores = await apiRequest('/promotores');
        const container = document.getElementById('listaPromotores');
        container.innerHTML = '';

        if (promotores.length === 0) {
            container.innerHTML = '<div class="col-12"><p class="text-muted">Nenhum promotor cadastrado.</p></div>';
            return;
        }

        promotores.forEach(p => {
            const statusBadge = p.ativo ? '<span class="badge bg-success">Ativo</span>' : '<span class="badge bg-secondary">Inativo</span>';
            const card = document.createElement('div');
            card.className = 'col-md-6 col-lg-4 mb-3';
            card.innerHTML = `
                <div class="card h-100 shadow-sm">
                    <div class="card-body">
                        <div class="d-flex align-items-center mb-3">
                            <div class="avatar rounded-circle bg-primary text-white d-flex justify-content-center align-items-center me-3" style="width:44px;height:44px;">${p.nome ? p.nome[0].toUpperCase() : '?'}</div>
                            <div>
                                <h5 class="card-title mb-1">${p.nome}</h5>
                                <small class="text-muted">${p.empresa?.razaoSocial || 'Sem empresa'}</small>
                            </div>
                        </div>
                        <p class="mb-1">CPF: ${p.cpf || '-'}</p>
                        <p class="mb-1">Categoria: ${p.categoria || 'Promotor'}</p>
                        <p class="mb-1">Dias permitidos: ${(p.diasPermitidos || '').replace(/,/g, ', ') || '-'}</p>
                        <p class="mb-1">Telefone: ${p.telefone || '-'}</p>
                        <p class="mb-3">Email: ${p.email || '-'}</p>
                        ${statusBadge}
                    </div>
                    <div class="card-footer bg-white border-0 d-flex justify-content-between">
                        <button class="btn btn-sm btn-outline-primary" onclick="editarPromotor(${p.id})">Editar</button>
                        ${p.ativo ? `<button class="btn btn-sm btn-danger" onclick="desativarPromotor(${p.id})">Desativar</button>` : ''}
                    </div>
                </div>
            `;
            container.appendChild(card);
        });
    } catch (error) {
        alert('Erro ao carregar promotores: ' + error.message);
    }
}

window.addEventListener('storage', (event) => {
    if (event.key === 'empresasUpdated') {
        carregarEmpresas();
    }
});

async function buscarCep() {
    const cep = document.getElementById('cep').value.replace(/\D/g, '');
    if (!cep) return;

    document.getElementById('logradouro').value = 'Buscando...';
}

function formatarCPF(cpf) {
    return cpf
        .replace(/\D/g, '')
        .slice(0, 11)
        .replace(/(\d{3})(\d)/, '$1.$2')
        .replace(/(\d{3})(\d)/, '$1.$2')
        .replace(/(\d{3})(\d{1,2})$/, '$1-$2');
}

function formatarCEP(cep) {
    return cep
        .replace(/\D/g, '')
        .slice(0, 8)
        .replace(/(\d{5})(\d{1,3})$/, '$1-$2');
}

function formatarTelefone(tel) {
    return tel
        .replace(/\D/g, '')
        .slice(0, 11)
        .replace(/(\d{2})(\d{5})(\d{0,4})/, '($1) $2-$3')
        .replace(/-$/, '');
}


    try {
        const response = await fetch(`https://viacep.com.br/ws/${cep}/json/`);
        const data = await response.json();

        if (data.erro) {
            alert('CEP não encontrado. Preencha manualmente.');
            document.getElementById('logradouro').value = '';
            return;
        }

        document.getElementById('logradouro').value = `${data.logradouro || ''}${data.bairro ? ', ' + data.bairro : ''} ${data.localidade || ''} - ${data.uf || ''}`.trim();
    } catch (error) {
        alert('Erro ao buscar CEP. Preencha manualmente.');
        document.getElementById('logradouro').value = '';
    }
}

function limparValidacaoPromotor() {
    const campos = [
        'nome', 'empresa', 'cpf', 'categoria', 'cep', 'logradouro', 'numero', 'complemento', 'telefone', 'email'
    ];
    campos.forEach(campo => {
        const el = document.getElementById(campo);
        if (el) {
            el.classList.remove('is-invalid', 'is-valid');
        }
        const feedback = el?.parentElement?.querySelector('.invalid-feedback');
        if (feedback) {
            feedback.remove();
        }
    });
    document.getElementById('mensagem').className = 'alert d-none';
}

function marcarErro(campoId, mensagem) {
    const el = document.getElementById(campoId);
    if (!el) return;
    el.classList.add('is-invalid');
    const feedback = document.createElement('div');
    feedback.className = 'invalid-feedback';
    feedback.textContent = mensagem;
    if (el.parentElement) {
        const existing = el.parentElement.querySelector('.invalid-feedback');
        if (existing) existing.remove();
        el.parentElement.appendChild(feedback);
    }
}

function marcarValido(campoId) {
    const el = document.getElementById(campoId);
    if (!el) return;
    el.classList.remove('is-invalid');
    el.classList.add('is-valid');
    const existing = el.parentElement?.querySelector('.invalid-feedback');
    if (existing) existing.remove();
}

function validarPromotor() {
    limparValidacaoPromotor();

    let valido = true;
    const nome = document.getElementById('nome').value.trim();
    const empresa = document.getElementById('empresa').value;
    const cpf = document.getElementById('cpf').value.trim();
    const email = document.getElementById('email').value.trim();

    if (!nome) {
        valido = false;
        marcarErro('nome', 'Informe o nome completo.');
    } else {
        marcarValido('nome');
    }

    if (!empresa) {
        valido = false;
        marcarErro('empresa', 'Selecione a empresa.');
    } else {
        marcarValido('empresa');
    }

    if (!cpf) {
        valido = false;
        marcarErro('cpf', 'Informe o CPF.');
    } else {
        marcarValido('cpf');
    }

    if (email && !/^\S+@\S+\.\S+$/.test(email)) {
        valido = false;
        marcarErro('email', 'E-mail inválido.');
    } else if (email) {
        marcarValido('email');
    }

    return valido;
}

async function salvarPromotor(e) {
    e.preventDefault();

    if (!validarPromotor()) {
        const mensagem = document.getElementById('mensagem');
        mensagem.className = 'alert alert-danger';
        mensagem.textContent = 'Por favor, corrija os campos em destaque.';
        return;
    }

    const id = document.getElementById('promotorId').value;
    const nome = document.getElementById('nome').value.trim();
    const cpf = document.getElementById('cpf').value.trim();
    const categoria = document.getElementById('categoria').value;
    const cep = document.getElementById('cep').value.trim();
    const logradouro = document.getElementById('logradouro').value.trim();
    const numero = document.getElementById('numero').value.trim();
    const complemento = document.getElementById('complemento').value.trim();
    const telefone = document.getElementById('telefone').value;
    const email = document.getElementById('email').value;
    const empresaId = document.getElementById('empresa').value;
    const diasPermitidos = Array.from(document.querySelectorAll('input[name="diasSemana"]:checked')).map(c => c.value);

    const enderecoCompleto = `${logradouro}${numero ? ', ' + numero : ''}${complemento ? ' - ' + complemento : ''}`.trim();

    const dados = {
        nome,
        cpf,
        categoria,
        endereco: enderecoCompleto,
        telefone,
        email,
        empresaId: parseInt(empresaId),
        diasPermitidos: diasPermitidos.join(','),
        ativo: true,
        cep
    };
    try {
        if (id) {
            await apiRequest(`/promotores/${id}`, 'PUT', dados);
        } else {
            await apiRequest('/promotores', 'POST', dados);
        }
        limparForm();
        await carregarPromotores();
        if (window.modalPromotor) window.modalPromotor.hide();
    } catch (error) {
        alert('Erro ao salvar promotor: ' + error.message);
    }
}

window.editarPromotor = async (id) => {
    try {
        const promotores = await apiRequest('/promotores');
        const promotor = promotores.find(p => p.id === id);
        if (promotor) {
            document.getElementById('promotorId').value = promotor.id;
            document.getElementById('nome').value = promotor.nome;
            document.getElementById('cpf').value = promotor.cpf || '';
            document.getElementById('categoria').value = promotor.categoria || 'Promotor';
            document.getElementById('cep').value = promotor.cep || '';
            document.getElementById('logradouro').value = promotor.endereco || '';
            document.getElementById('numero').value = promotor.numero || '';
            document.getElementById('complemento').value = promotor.complemento || '';
            document.getElementById('telefone').value = promotor.telefone || '';
            document.getElementById('email').value = promotor.email || '';
            document.getElementById('empresa').value = promotor.empresaId;

            const diasSelecionados = (promotor.diasPermitidos || '').split(',').map(x => x.trim());
            document.querySelectorAll('input[name="diasSemana"]').forEach(input => {
                input.checked = diasSelecionados.includes(input.value);
            });

            document.getElementById('modalPromotorLabel').textContent = 'Editar Promotor';
            document.getElementById('btnSalvar').textContent = 'Atualizar';
            if (window.modalPromotor) window.modalPromotor.show();
        }
    } catch (error) {
        alert('Erro ao carregar promotor: ' + error.message);
    }
};

window.desativarPromotor = async (id) => {
    if (!confirm('Tem certeza que deseja desativar este promotor?')) return;
    try {
        await apiRequest(`/promotores/${id}`, 'DELETE');
        await carregarPromotores();
    } catch (error) {
        alert('Erro ao desativar promotor: ' + error.message);
    }
};

function cancelarEdicao() {
    limparForm();
}

function limparForm() {
    document.getElementById('promotorId').value = '';
    document.getElementById('nome').value = '';
    document.getElementById('cpf').value = '';
    document.getElementById('categoria').value = 'Promotor';
    document.getElementById('cep').value = '';
    document.getElementById('logradouro').value = '';
    document.getElementById('numero').value = '';
    document.getElementById('complemento').value = '';
    document.getElementById('telefone').value = '';
    document.getElementById('email').value = '';
    document.getElementById('empresa').value = '';
    document.querySelectorAll('input[name="diasSemana"]').forEach(input => input.checked = false);
    document.getElementById('modalPromotorLabel').textContent = 'Novo Promotor';
    document.getElementById('btnSalvar').textContent = 'Cadastrar';
    document.getElementById('mensagem').className = 'alert d-none';
    limparValidacaoPromotor();
}