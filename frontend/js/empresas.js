// empresas.js - Gerencia CRUD de empresas

let empresasLista = [];
const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;

document.addEventListener('DOMContentLoaded', async function() {
    requireAuth();
    await carregarEmpresas();
    configurarMascarasEmpresa();
    
    const formEmpresa = document.getElementById('formEmpresa');
    if (formEmpresa) {
        formEmpresa.addEventListener('submit', handleSalvarEmpresa);
    }
    
    const btnCancelar = document.getElementById('btnCancelarEmpresa');
    if (btnCancelar) {
        btnCancelar.addEventListener('click', limparFormulario);
    }

    const filtroEmpresa = document.getElementById('filtroEmpresa');
    if (filtroEmpresa) {
        filtroEmpresa.addEventListener('input', renderizarEmpresas);
    }
    
    // Mascara e validacao ficam locais a tela para evitar listeners duplicados.
});

async function carregarEmpresas() {
    try {
        empresasLista = await getEmpresas();
        renderizarEmpresas();
    } catch (error) {
        console.error('Erro ao carregar empresas:', error);
    }
}

function getInputValue(id) {
    return document.getElementById(id)?.value.trim() || '';
}

function somenteDigitos(valor, limite) {
    const digits = String(valor || '').replace(/\D/g, '');
    return limite ? digits.slice(0, limite) : digits;
}

function normalizarBusca(valor) {
    return String(valor || '')
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .toLowerCase()
        .trim();
}

function aplicarMascaraCNPJ(valor) {
    const digits = somenteDigitos(valor, 14);

    if (digits.length <= 2) return digits;
    if (digits.length <= 5) return `${digits.slice(0, 2)}.${digits.slice(2)}`;
    if (digits.length <= 8) return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5)}`;
    if (digits.length <= 12) return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5, 8)}/${digits.slice(8)}`;

    return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5, 8)}/${digits.slice(8, 12)}-${digits.slice(12)}`;
}

function formatarCNPJ(cnpj) {
    const digits = somenteDigitos(cnpj, 14);
    if (digits.length !== 14) return cnpj.trim();

    return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5, 8)}/${digits.slice(8, 12)}-${digits.slice(12)}`;
}

function aplicarMascaraCEP(valor) {
    const digits = somenteDigitos(valor, 8);
    if (digits.length <= 5) return digits;
    return `${digits.slice(0, 5)}-${digits.slice(5)}`;
}

function aplicarMascaraTelefone(valor) {
    const digits = somenteDigitos(valor, 11);

    if (!digits) return '';
    if (digits.length <= 2) return `(${digits}`;
    if (digits.length <= 6) return `(${digits.slice(0, 2)}) ${digits.slice(2)}`;
    if (digits.length <= 10) {
        return `(${digits.slice(0, 2)}) ${digits.slice(2, 6)}-${digits.slice(6)}`;
    }

    return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7)}`;
}

function formatarTelefoneExibicao(telefone) {
    const digits = somenteDigitos(telefone, 11);

    if (!digits) return '-';
    if (digits.length === 8) return `${digits.slice(0, 4)}-${digits.slice(4)}`;
    if (digits.length === 9) return `${digits.slice(0, 5)}-${digits.slice(5)}`;
    if (digits.length >= 10) return aplicarMascaraTelefone(digits);

    return telefone || '-';
}

function normalizarEmail(valor) {
    return String(valor || '').trim().replace(/\s+/g, '').slice(0, 150);
}

function normalizarUF(valor) {
    return String(valor || '').replace(/[^A-Za-z]/g, '').slice(0, 2).toUpperCase();
}

function feedbackAnchor(input) {
    return input?.closest('.input-group') || input;
}

function getFeedbackElement(input) {
    if (!input?.id) return null;

    const anchor = feedbackAnchor(input);
    const parent = anchor?.parentElement;
    if (!parent) return null;

    const id = `${input.id}Feedback`;
    let feedback = parent.querySelector(`#${id}`);

    if (!feedback) {
        feedback = document.createElement('small');
        feedback.id = id;
        feedback.className = 'validation-feedback d-none';
        anchor.insertAdjacentElement('afterend', feedback);
    }

    return feedback;
}

function limparFeedback(input) {
    input?.classList.remove('erro', 'sucesso');
    input?.setCustomValidity('');

    const feedback = getFeedbackElement(input);
    if (feedback) {
        feedback.textContent = '';
        feedback.classList.add('d-none');
    }
}

function marcarCampoInvalido(input, mensagem) {
    if (!input) return false;

    input.classList.remove('sucesso');
    input.classList.add('erro');
    input.setCustomValidity(mensagem);

    const feedback = getFeedbackElement(input);
    if (feedback) {
        feedback.textContent = mensagem;
        feedback.classList.remove('d-none');
    }

    return false;
}

function marcarCampoAviso(input, mensagem) {
    if (!input) return false;

    input.classList.remove('sucesso');
    input.classList.add('erro');
    input.setCustomValidity('');

    const feedback = getFeedbackElement(input);
    if (feedback) {
        feedback.textContent = mensagem;
        feedback.classList.remove('d-none');
    }

    return false;
}

function marcarCampoValido(input) {
    if (!input) return true;

    input.classList.remove('erro');
    if (input.value.trim()) input.classList.add('sucesso');
    input.setCustomValidity('');

    const feedback = getFeedbackElement(input);
    if (feedback) {
        feedback.textContent = '';
        feedback.classList.add('d-none');
    }

    return true;
}

function validarEmailInput(input, obrigatorio = false) {
    if (!input) return true;

    input.value = normalizarEmail(input.value);
    if (!input.value) {
        return obrigatorio
            ? marcarCampoInvalido(input, 'Informe um email valido.')
            : marcarCampoValido(input);
    }

    return EMAIL_REGEX.test(input.value)
        ? marcarCampoValido(input)
        : marcarCampoInvalido(input, 'Use um email valido, como nome@empresa.com.');
}

function validarTelefoneInput(input, obrigatorio = false) {
    if (!input) return true;

    const digits = somenteDigitos(input.value, 11);
    input.value = aplicarMascaraTelefone(digits);

    if (!digits) {
        return obrigatorio
            ? marcarCampoInvalido(input, 'Informe um telefone.')
            : marcarCampoValido(input);
    }

    return digits.length >= 10
        ? marcarCampoValido(input)
        : marcarCampoInvalido(input, 'Informe DDD e numero do telefone.');
}

function validarCNPJInput(input) {
    if (!input) return true;

    const digits = somenteDigitos(input.value, 14);
    input.value = aplicarMascaraCNPJ(digits);

    return digits.length === 14
        ? marcarCampoValido(input)
        : marcarCampoInvalido(input, 'CNPJ deve ter 14 digitos.');
}

function validarCepInput(input) {
    if (!input) return true;

    const digits = somenteDigitos(input.value, 8);
    input.value = aplicarMascaraCEP(digits);

    if (!digits) {
        limparFeedback(input);
        return true;
    }

    return digits.length === 8
        ? marcarCampoValido(input)
        : marcarCampoAviso(input, 'CEP deve ter 8 digitos.');
}

function configurarMascarasEmpresa() {
    const cnpjInput = document.getElementById('cnpj');
    const cepInput = document.getElementById('cepEmpresa');
    const telefoneInput = document.getElementById('telefoneEmpresa');
    const emailInput = document.getElementById('emailEmpresa');
    const numeroInput = document.getElementById('numeroEmpresa');
    const estadoInput = document.getElementById('estadoEmpresa');

    cnpjInput?.addEventListener('input', () => {
        cnpjInput.value = aplicarMascaraCNPJ(cnpjInput.value);
        limparFeedback(cnpjInput);
        if (somenteDigitos(cnpjInput.value).length === 14) buscarCNPJ();
    });
    cnpjInput?.addEventListener('blur', () => {
        if (cnpjInput.value) validarCNPJInput(cnpjInput);
    });

    cepInput?.addEventListener('input', () => {
        cepInput.value = aplicarMascaraCEP(cepInput.value);
        limparFeedback(cepInput);
    });
    cepInput?.addEventListener('blur', () => {
        const digits = somenteDigitos(cepInput.value, 8);
        if (!digits) {
            limparFeedback(cepInput);
            return;
        }
        if (validarCepInput(cepInput)) buscarCep();
    });

    telefoneInput?.addEventListener('input', () => {
        telefoneInput.value = aplicarMascaraTelefone(telefoneInput.value);
        limparFeedback(telefoneInput);
    });
    telefoneInput?.addEventListener('blur', () => validarTelefoneInput(telefoneInput, true));

    emailInput?.addEventListener('input', () => {
        emailInput.value = normalizarEmail(emailInput.value);
        limparFeedback(emailInput);
    });
    emailInput?.addEventListener('blur', () => validarEmailInput(emailInput, true));

    numeroInput?.addEventListener('input', () => {
        numeroInput.value = somenteDigitos(numeroInput.value, 6);
    });

    estadoInput?.addEventListener('input', () => {
        estadoInput.value = normalizarUF(estadoInput.value);
    });
}

function montarEnderecoConsolidado() {
    const logradouro = getInputValue('logradouroEmpresa');
    const numero = getInputValue('numeroEmpresa');
    const cidade = getInputValue('cidadeEmpresa');
    const estado = getInputValue('estadoEmpresa');
    const complemento = getInputValue('complementoEmpresa');

    return [logradouro, numero, cidade, estado, complemento]
        .filter(Boolean)
        .join(', ');
}

function renderizarEmpresas() {
    const tbody = document.getElementById('tbodyEmpresas');
    if (!tbody) return;

    const filtro = normalizarBusca(document.getElementById('filtroEmpresa')?.value);
    const filtroDigits = somenteDigitos(filtro);
    const empresasFiltradas = empresasLista.filter(empresa => {
        if (!filtro) return true;

        const campos = [
            empresa.razaoSocial,
            empresa.nomeFantasia,
            empresa.cnpj,
            empresa.email
        ].map(normalizarBusca);

        const cnpjDigits = somenteDigitos(empresa.cnpj);

        return campos.some(campo => campo.includes(filtro))
            || (!!filtroDigits && cnpjDigits.includes(filtroDigits));
    });
    
    tbody.innerHTML = '';

    if (empresasFiltradas.length === 0) {
        const mensagem = filtro
            ? 'Nenhuma empresa encontrada'
            : 'Nenhuma empresa cadastrada';
        const detalhe = filtro
            ? 'Revise o termo buscado ou limpe o filtro para ver todas as empresas.'
            : 'Cadastre uma empresa para que ela apareça nesta lista.';

        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-5">
                    <div class="text-light fw-semibold mb-1">${mensagem}</div>
                    <small class="text-muted">${detalhe}</small>
                </td>
            </tr>
        `;
        return;
    }
    
    empresasFiltradas.forEach(empresa => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${empresa.cnpj}</td>
            <td>${empresa.razaoSocial}</td>
            <td>${empresa.nomeFantasia || '-'}</td>
            <td>${formatarTelefoneExibicao(empresa.telefone)}</td>
            <td>${empresa.email || '-'}</td>
            <td>${empresa.endereco || '-'}</td>
            <td>
                <button class="btn btn-sm btn-edit" onclick="editarEmpresa(${empresa.id})">Editar</button>
                <button class="btn btn-sm btn-delete" onclick="deletarEmpresa(${empresa.id})">Deletar</button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

async function handleSalvarEmpresa(e) {
    e.preventDefault();
    
    const id = document.getElementById('empresaId').value;
    const cnpjInput = document.getElementById('cnpj');
    const telefoneInput = document.getElementById('telefoneEmpresa');
    const emailInput = document.getElementById('emailEmpresa');
    const endereco = montarEnderecoConsolidado();

    if (!validarCNPJInput(cnpjInput)) {
        cnpjInput.focus();
        return;
    }

    if (!validarTelefoneInput(telefoneInput, true)) {
        telefoneInput.focus();
        return;
    }

    if (!validarEmailInput(emailInput, true)) {
        emailInput.focus();
        return;
    }

    if (!endereco) {
        alert('Informe o endereço da empresa');
        return;
    }

    const data = {
        cnpj: formatarCNPJ(getInputValue('cnpj')),
        razaoSocial: getInputValue('razaoSocial'),
        nomeFantasia: getInputValue('nomeFantasia'),
        telefone: telefoneInput.value.trim(),
        email: emailInput.value.trim(),
        endereco
    };
    
    try {
        if (id) {
            await updateEmpresa(id, data);
        } else {
            await createEmpresa(data);
        }
        
        await carregarEmpresas();
        limparFormulario();
    } catch (error) {
        console.error('Erro ao salvar:', error);
        alert('Erro ao salvar empresa');
    }
}

function limparFormulario() {
    document.getElementById('formEmpresa').reset();
    document.getElementById('empresaId').value = '';
    ['cnpj', 'telefoneEmpresa', 'emailEmpresa', 'cepEmpresa'].forEach(id => {
        limparFeedback(document.getElementById(id));
    });
}

async function editarEmpresa(id) {
    try {
        const empresa = await getEmpresa(id);
        document.getElementById('empresaId').value = empresa.id;
        document.getElementById('cnpj').value = aplicarMascaraCNPJ(empresa.cnpj);
        document.getElementById('razaoSocial').value = empresa.razaoSocial;
        document.getElementById('nomeFantasia').value = empresa.nomeFantasia || '';
        document.getElementById('telefoneEmpresa').value = aplicarMascaraTelefone(empresa.telefone || '');
        document.getElementById('emailEmpresa').value = normalizarEmail(empresa.email || '');
        document.getElementById('logradouroEmpresa').value = empresa.endereco || '';
        document.getElementById('numeroEmpresa').value = '';
        document.getElementById('cidadeEmpresa').value = '';
        document.getElementById('estadoEmpresa').value = '';
        document.getElementById('complementoEmpresa').value = '';
        ['cnpj', 'telefoneEmpresa', 'emailEmpresa', 'cepEmpresa'].forEach(inputId => {
            limparFeedback(document.getElementById(inputId));
        });
    } catch (error) {
        console.error('Erro ao editar:', error);
    }
}

async function deletarEmpresa(id) {
    if (confirm('Tem certeza que deseja deletar esta empresa?')) {
        try {
            await deleteEmpresa(id);
            await carregarEmpresas();
        } catch (error) {
            console.error('Erro ao deletar:', error);
            alert('Erro ao deletar empresa');
        }
    }
}
function buscarCep() {
    const cepInput = document.getElementById('cepEmpresa');
    const logradouro = document.getElementById('logradouroEmpresa');
    const cidade = document.getElementById('cidadeEmpresa');
    const estado = document.getElementById('estadoEmpresa');
    
    if (!cepInput) return;

    const cep = somenteDigitos(cepInput.value, 8);
    cepInput.value = aplicarMascaraCEP(cep);

    limparFeedback(cepInput);

    if (!cep) {
        return;
    }

    if (cep.length !== 8) {
        marcarCampoAviso(cepInput, 'CEP deve ter 8 digitos.');
        return;
    }

    marcarCampoValido(cepInput);
    logradouro?.classList.add('loading');
    if (logradouro) logradouro.value = 'Buscando...';

    fetch(`https://viacep.com.br/ws/${cep}/json/`)
        .then(res => res.json())
        .then(data => {
            logradouro?.classList.remove('loading');

            if (data.erro) {
                marcarCampoAviso(cepInput, 'CEP nao encontrado. Voce pode preencher o endereco manualmente.');
                if (logradouro) logradouro.value = '';
                return;
            }

            if (logradouro && data.logradouro) logradouro.value = data.logradouro;
            if (cidade && data.localidade) cidade.value = data.localidade;
            if (estado && data.uf) estado.value = data.uf;
            marcarCampoValido(cepInput);
        })
        .catch(() => {
            logradouro?.classList.remove('loading');
            if (logradouro) logradouro.value = '';
            marcarCampoAviso(cepInput, 'Nao foi possivel buscar o CEP. Preencha o endereco manualmente.');
        });
}

function buscarCNPJ() {
    const cnpjInput = document.getElementById('cnpj');
    const razaoSocial = document.getElementById('razaoSocial');
    
    if (!cnpjInput) return;

    const cnpj = somenteDigitos(cnpjInput.value, 14);
    cnpjInput.value = aplicarMascaraCNPJ(cnpj);

    limparFeedback(cnpjInput);

    if (cnpj.length !== 14) {
        marcarCampoInvalido(cnpjInput, 'CNPJ deve ter 14 digitos.');
        return;
    }

    marcarCampoValido(cnpjInput);
    razaoSocial?.classList.add('loading');
    if (razaoSocial) razaoSocial.value = 'Buscando...';

    fetch(`https://brasilapi.com.br/api/cnpj/v1/${cnpj}`)
        .then(res => {
            if (!res.ok) throw new Error('Não encontrado');
            return res.json();
        })
        .then(data => {
            razaoSocial?.classList.remove('loading');

            if (!data || data.message || data.status === 'ERROR') {
                marcarCampoAviso(cnpjInput, 'CNPJ nao encontrado. Confira o numero ou preencha os dados manualmente.');
                if (razaoSocial) razaoSocial.value = '';
                return;
            }

            // Preenche campos disponíveis
            if (razaoSocial) razaoSocial.value = data.razao_social || data.nome || '';
            const nomeFantasiaEl = document.getElementById('nomeFantasia');
            if (nomeFantasiaEl) nomeFantasiaEl.value = data.nome_fantasia || '';

            // Alguns serviços retornam endereço em objeto diferente
            try {
                const logradouroEl = document.getElementById('logradouroEmpresa');
                const cidadeEl = document.getElementById('cidadeEmpresa');
                const estadoEl = document.getElementById('estadoEmpresa');

                if (data.estabelecimento) {
                    if (logradouroEl && data.estabelecimento.logradouro) logradouroEl.value = data.estabelecimento.logradouro;
                    if (cidadeEl && data.estabelecimento.municipio) cidadeEl.value = data.estabelecimento.municipio;
                    if (estadoEl && data.estabelecimento.uf) estadoEl.value = data.estabelecimento.uf;
                }
            } catch (e) {
                // ignore
            }

            marcarCampoValido(cnpjInput);
        })
        .catch(() => {
            razaoSocial?.classList.remove('loading');
            if (razaoSocial) razaoSocial.value = '';
            marcarCampoAviso(cnpjInput, 'Erro ao buscar CNPJ. Confira o numero ou preencha os dados manualmente.');
        });
}

window.buscarCep = buscarCep;
window.buscarCNPJ = buscarCNPJ;
