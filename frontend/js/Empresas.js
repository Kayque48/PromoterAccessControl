document.addEventListener('DOMContentLoaded', async () => {
    await carregarEmpresas();

    const formEmpresa = document.getElementById('formEmpresa');
    formEmpresa.addEventListener('submit', salvarEmpresa);

    document.getElementById('btnCancelarEmpresa').addEventListener('click', () => {
        limparFormularioEmpresa();
    });

    const btnBuscarCepEmpresa = document.getElementById('btnBuscarCepEmpresa');
    if (btnBuscarCepEmpresa) {
        btnBuscarCepEmpresa.addEventListener('click', buscarCepEmpresa);
    }

    const cepEmpresaInput = document.getElementById('cepEmpresa');
    if (cepEmpresaInput) {
        cepEmpresaInput.addEventListener('blur', buscarCepEmpresa);
        cepEmpresaInput.addEventListener('input', (e) => e.target.value = formatarCEP(e.target.value));
    }

    const cnpjInput = document.getElementById('cnpj');
    if (cnpjInput) {
        cnpjInput.addEventListener('input', (e) => e.target.value = formatarCNPJ(e.target.value));
    }

    const telefoneEmpresaInput = document.getElementById('telefoneEmpresa');
    if (telefoneEmpresaInput) {
        telefoneEmpresaInput.addEventListener('input', (e) => e.target.value = formatarTelefone(e.target.value));
    }
});

async function buscarCepEmpresa() {
    const cep = document.getElementById('cepEmpresa').value.replace(/\D/g, '');
    if (!cep) return;

    document.getElementById('logradouroEmpresa').value = 'Buscando...';
}

function formatarCNPJ(cnpj) {
    return cnpj
        .replace(/\D/g, '')
        .slice(0, 14)
        .replace(/(\d{2})(\d)/, '$1.$2')
        .replace(/(\d{3})(\d)/, '$1.$2')
        .replace(/(\d{3})(\d)/, '$1/$2')
        .replace(/(\d{4})(\d{1,2})$/, '$1-$2');
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
            document.getElementById('logradouroEmpresa').value = '';
            return;
        }

        document.getElementById('logradouroEmpresa').value = `${data.logradouro || ''}${data.bairro ? ', ' + data.bairro : ''} ${data.localidade || ''} - ${data.uf || ''}`.trim();
    } catch (error) {
        alert('Erro ao buscar CEP. Preencha manualmente.');
        document.getElementById('logradouroEmpresa').value = '';
    }


function limparValidacaoEmpresa() {
    const campos = ['cnpj', 'razaoSocial', 'cepEmpresa', 'logradouroEmpresa', 'numeroEmpresa', 'complementoEmpresa', 'telefoneEmpresa', 'emailEmpresa'];
    campos.forEach(campo => {
        const el = document.getElementById(campo);
        if (el) {
            el.classList.remove('is-invalid', 'is-valid');
            const feedback = el.parentElement?.querySelector('.invalid-feedback');
            if (feedback) feedback.remove();
        }
    });
}

function marcarErroEmpresa(campoId, mensagem) {
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

function marcarValidoEmpresa(campoId) {
    const el = document.getElementById(campoId);
    if (!el) return;
    el.classList.remove('is-invalid');
    el.classList.add('is-valid');
    const existing = el.parentElement?.querySelector('.invalid-feedback');
    if (existing) existing.remove();
}

function validarEmpresa() {
    limparValidacaoEmpresa();

    let valido = true;
    const cnpj = document.getElementById('cnpj').value.trim();
    const razaoSocial = document.getElementById('razaoSocial').value.trim();
    const email = document.getElementById('emailEmpresa').value.trim();

    if (!cnpj) {
        valido = false;
        marcarErroEmpresa('cnpj', 'Informe o CNPJ.');
    } else {
        marcarValidoEmpresa('cnpj');
    }

    if (!razaoSocial) {
        valido = false;
        marcarErroEmpresa('razaoSocial', 'Informe a razão social.');
    } else {
        marcarValidoEmpresa('razaoSocial');
    }

    if (email && !/^\S+@\S+\.\S+$/.test(email)) {
        valido = false;
        marcarErroEmpresa('emailEmpresa', 'E-mail inválido.');
    } else if (email) {
        marcarValidoEmpresa('emailEmpresa');
    }

    return valido;
}

async function carregarEmpresas() {
    try {
        if (!localStorage.getItem('role') || localStorage.getItem('role') !== 'Gestor') {
            checkEmpresaAccess();
            return;
        }

        let empresas = await apiRequest('/promotores/empresas');
        const filtro = document.getElementById('filtroEmpresa').value.trim().toLowerCase();
        if (filtro) {
            empresas = empresas.filter(e =>
                (e.razaoSocial || '').toLowerCase().includes(filtro) ||
                (e.nomeFantasia || '').toLowerCase().includes(filtro) ||
                (e.cnpj || '').toLowerCase().includes(filtro)
            );
        }

        const tbody = document.getElementById('tbodyEmpresas');
        tbody.innerHTML = '';

        if (empresas.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4" class="text-center text-muted">Nenhuma empresa cadastrada.</td></tr>';
            return;
        }

        empresas.forEach(empresa => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${empresa.cnpj}</td>
                <td>${empresa.razaoSocial}</td>
                <td>${empresa.nomeFantasia || '-'}</td>
                <td>${empresa.telefone || '-'}</td>
                <td>${empresa.emailCorporativo || '-'}</td>
                <td>${empresa.endereco || '-'}</td>
                <td>
                    <button class="btn btn-sm btn-outline-primary me-1" onclick="editarEmpresa(${empresa.id})"><i class="bi bi-pencil-square"></i></button>
                    <button class="btn btn-sm btn-outline-danger" onclick="excluirEmpresa(${empresa.id})"><i class="bi bi-trash"></i></button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    } catch (error) {
        alert('Erro ao carregar empresas: ' + error.message);
    }
}

window.addEventListener('DOMContentLoaded', () => {
    const filtroInput = document.getElementById('filtroEmpresa');
    filtroInput.addEventListener('input', carregarEmpresas);
    checkEmpresaAccess();
});

async function salvarEmpresa(event) {
    event.preventDefault();

    if (!validarEmpresa()) {
        showToast('Por favor corrija os campos em destaque.', 'danger');
        return;
    }

    const id = document.getElementById('empresaId').value;
    const cnpj = document.getElementById('cnpj').value.trim();
    const razaoSocial = document.getElementById('razaoSocial').value.trim();
    const nomeFantasia = document.getElementById('nomeFantasia').value.trim();
    const telefone = document.getElementById('telefoneEmpresa').value.trim();
    const email = document.getElementById('emailEmpresa').value.trim();
    const cep = document.getElementById('cepEmpresa').value.trim();
    const logradouro = document.getElementById('logradouroEmpresa').value.trim();
    const numero = document.getElementById('numeroEmpresa').value.trim();
    const complemento = document.getElementById('complementoEmpresa').value.trim();

    if (!cnpj || !razaoSocial) {
        alert('Preencha CNPJ e Razão Social.');
        return;
    }

    const endereco = `${logradouro}${numero ? ', ' + numero : ''}${complemento ? ' - ' + complemento : ''}`.trim();

    const payload = {
        cnpj,
        razaoSocial,
        nomeFantasia: nomeFantasia || null,
        telefone: telefone || null,
        emailCorporativo: email || null,
        endereco: endereco || null,
        cep: cep || null
    };
    try {
        if (id) {
            await apiRequest(`/promotores/empresas/${id}`, 'PUT', { ...payload, id: parseInt(id) });
            showToast('Empresa atualizada com sucesso!', 'success');
        } else {
            await apiRequest('/promotores/empresas', 'POST', payload);
            showToast('Empresa cadastrada com sucesso!', 'success');
        }
        localStorage.setItem('empresasUpdated', Date.now());
        limparFormularioEmpresa();
        await carregarEmpresas();
    } catch (error) {
        showToast('Erro ao salvar empresa: ' + error.message, 'danger');
    }
}

window.editarEmpresa = async (id) => {
    try {
        const empresas = await apiRequest('/promotores/empresas');
        const empresa = empresas.find(e => e.id === id);
        if (!empresa) {
            alert('Empresa não encontrada.');
            return;
        }

        document.getElementById('empresaId').value = empresa.id;
        document.getElementById('cnpj').value = empresa.cnpj;
        document.getElementById('razaoSocial').value = empresa.razaoSocial;
        document.getElementById('nomeFantasia').value = empresa.nomeFantasia || '';
        document.getElementById('telefoneEmpresa').value = empresa.telefone || '';
        document.getElementById('emailEmpresa').value = empresa.emailCorporativo || '';
        document.getElementById('cepEmpresa').value = empresa.cep || '';
        document.getElementById('logradouroEmpresa').value = empresa.endereco || '';
        document.getElementById('numeroEmpresa').value = (empresa.endereco || '').match(/\d+/) ? (empresa.endereco || '').match(/\d+/)[0] : '';
        document.getElementById('complementoEmpresa').value = (empresa.endereco || '').split(' - ')[1] || '';
        document.getElementById('btnSalvarEmpresa').textContent = 'Atualizar';
    } catch (error) {
        alert('Erro ao buscar empresa: ' + error.message);
    }
};

window.excluirEmpresa = async (id) => {
    if (!confirm('Tem certeza que deseja excluir esta empresa?')) return;
    try {
        await apiRequest(`/promotores/empresas/${id}`, 'DELETE');
        await carregarEmpresas();
    } catch (error) {
        showToast('Erro ao excluir empresa: ' + error.message, 'danger');
    }
};

function limparFormularioEmpresa() {
    document.getElementById('empresaId').value = '';
    document.getElementById('cnpj').value = '';
    document.getElementById('razaoSocial').value = '';
    document.getElementById('nomeFantasia').value = '';
    document.getElementById('telefoneEmpresa').value = '';
    document.getElementById('emailEmpresa').value = '';
    document.getElementById('cepEmpresa').value = '';
    document.getElementById('logradouroEmpresa').value = '';
    document.getElementById('numeroEmpresa').value = '';
    document.getElementById('complementoEmpresa').value = '';
    document.getElementById('btnSalvarEmpresa').textContent = 'Salvar';
}
