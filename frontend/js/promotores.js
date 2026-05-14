// promotores.js - Gerencia CRUD de promotores

let promotoresLista = [];
let empresasPromotorLista = [];
const EMAIL_PROMOTOR_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;

document.addEventListener('DOMContentLoaded', async function() {
    // Verifica autenticação
    requireAuth();

    // Carrega empresas para select
    await carregarEmpresasSelectPromotor();

    // Carrega promotores
    await carregarPromotores();

    // Listeners de formulário
    const formPromotor = document.getElementById('formPromotor');
    if (formPromotor) {
        formPromotor.addEventListener('submit', handleSalvarPromotor);
    }

    const btnNovoPromotor = document.getElementById('btnNovoPromotor');
    if (btnNovoPromotor) {
        btnNovoPromotor.addEventListener('click', limparFormularioPromotor);
    }

    const btnCancelarTopo = document.getElementById('btnCancelarEmpresa');
    if (btnCancelarTopo) {
        btnCancelarTopo.addEventListener('click', () => {
            bootstrap.Modal.getInstance(document.getElementById('modalPromotor'))?.hide();
            limparFormularioPromotor();
        });
    }

    const filtroPromotor = document.getElementById('filtroPromotor');
    if (filtroPromotor) {
        filtroPromotor.addEventListener('input', renderizarPromotores);
    }

    configurarMascarasPromotor();
});

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

    return EMAIL_PROMOTOR_REGEX.test(input.value)
        ? marcarCampoValido(input)
        : marcarCampoInvalido(input, 'Use um email valido, como nome@email.com.');
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

function configurarMascarasPromotor() {
    const cpfInput = document.getElementById('cpf');
    const cepInput = document.getElementById('cepPromotor');
    const telefoneInput = document.getElementById('telefone');
    const emailInput = document.getElementById('email');
    const numeroInput = document.getElementById('numeroPromotor');
    const estadoInput = document.getElementById('estadoPromotor');
    const btnBuscarCep = document.getElementById('btnBuscarCepPromotor');

    cpfInput?.addEventListener('input', () => {
        cpfInput.value = aplicarMascaraCPF(cpfInput.value);
        limparFeedback(cpfInput);
    });
    cpfInput?.addEventListener('blur', () => validarCPFInput(cpfInput));

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
        if (validarCepInput(cepInput)) buscarCepPromotor();
    });
    btnBuscarCep?.addEventListener('click', buscarCepPromotor);

    telefoneInput?.addEventListener('input', () => {
        telefoneInput.value = aplicarMascaraTelefone(telefoneInput.value);
        limparFeedback(telefoneInput);
    });
    telefoneInput?.addEventListener('blur', () => validarTelefoneInput(telefoneInput, false));

    emailInput?.addEventListener('input', () => {
        emailInput.value = normalizarEmail(emailInput.value);
        limparFeedback(emailInput);
    });
    emailInput?.addEventListener('blur', () => validarEmailInput(emailInput, false));

    numeroInput?.addEventListener('input', () => {
        numeroInput.value = somenteDigitos(numeroInput.value, 6);
    });

    estadoInput?.addEventListener('input', () => {
        estadoInput.value = normalizarUF(estadoInput.value);
    });
}

async function carregarEmpresasSelectPromotor() {
    try {
        empresasPromotorLista = await getEmpresas();
        const selectEmpresa = document.getElementById('empresa');
        if (selectEmpresa) {
            selectEmpresa.innerHTML = '<option value="">Selecione...</option>';

            empresasPromotorLista.forEach(emp => {
                const option = document.createElement('option');
                option.value = emp.id;
                option.textContent = emp.nomeFantasia || emp.razaoSocial;
                selectEmpresa.appendChild(option);
            });
        }
    } catch (error) {
        console.error('Erro ao carregar empresas:', error);
    }
}

const diasPermitidosMap = {
    Sunday: 'domingo',
    Monday: 'segunda',
    Tuesday: 'ter\u00e7a',
    Wednesday: 'quarta',
    Thursday: 'quinta',
    Friday: 'sexta',
    Saturday: 's\u00e1bado',
    domingo: 'domingo',
    segunda: 'segunda',
    terca: 'ter\u00e7a',
    'ter\u00e7a': 'ter\u00e7a',
    quarta: 'quarta',
    quinta: 'quinta',
    sexta: 'sexta',
    sabado: 's\u00e1bado',
    's\u00e1bado': 's\u00e1bado'
};

function normalizarDiaPermitido(dia) {
    const chave = String(dia || '').trim().toLowerCase();
    const original = String(dia || '').trim();
    return diasPermitidosMap[original] || diasPermitidosMap[chave] || chave;
}

function getDiasPermitidosSelecionados() {
    return Array.from(document.querySelectorAll('input[name="diasSemana"]:checked'))
        .map(input => normalizarDiaPermitido(input.value))
        .filter(Boolean);
}

function setDiasPermitidosSelecionados(diasPermitidos) {
    const dias = new Set((diasPermitidos || []).map(normalizarDiaPermitido));

    document.querySelectorAll('input[name="diasSemana"]').forEach(input => {
        input.checked = dias.has(normalizarDiaPermitido(input.value));
    });
}

function getTipoSelecionado() {
    const categoria = document.getElementById('categoria')?.value || '';
    return categoria.toLowerCase().includes('exclusivo') ? 'exclusivo' : 'promotor';
}

function isPromotorExclusivo(promotor) {
    const tipo = String(promotor.tipo || '').toLowerCase();
    return tipo.includes('exclusivo') || !!promotor.empresaExclusivaId;
}

function getEmpresaIdPrincipal(promotor) {
    if (promotor.empresaId) return promotor.empresaId;
    if (promotor.empresaExclusivaId) return promotor.empresaExclusivaId;
    if (Array.isArray(promotor.empresaIds) && promotor.empresaIds.length > 0) {
        return promotor.empresaIds[0];
    }

    return '';
}

function getEmpresaNome(empresaId) {
    const empresa = empresasPromotorLista.find(emp => Number(emp.id) === Number(empresaId));
    return empresa ? (empresa.nomeFantasia || empresa.razaoSocial) : empresaId;
}

function formatarEmpresasPromotor(promotor) {
    const empresaIds = Array.isArray(promotor.empresaIds) && promotor.empresaIds.length > 0
        ? promotor.empresaIds
        : [getEmpresaIdPrincipal(promotor)].filter(Boolean);

    if (empresaIds.length === 0) return '-';
    return empresaIds.map(getEmpresaNome).join(', ');
}

function formatarDiasPermitidos(diasPermitidos) {
    if (!Array.isArray(diasPermitidos) || diasPermitidos.length === 0) return '-';
    return diasPermitidos.map(normalizarDiaPermitido).join(', ');
}

function formatarTipoPromotor(tipo) {
    return tipo === 'exclusivo' ? 'Promotor Exclusivo' : 'Promotor';
}

function formatarCPFExibicao(cpf) {
    const digits = somenteDigitos(cpf, 11);
    return digits.length === 11 ? aplicarMascaraCPF(digits) : (cpf || '-');
}

function formatarTelefoneExibicao(telefone) {
    const digits = somenteDigitos(telefone, 11);

    if (!digits) return '-';
    if (digits.length === 8) return `${digits.slice(0, 4)}-${digits.slice(4)}`;
    if (digits.length === 9) return `${digits.slice(0, 5)}-${digits.slice(5)}`;
    if (digits.length >= 10) return aplicarMascaraTelefone(digits);

    return telefone || '-';
}

function setModoFormularioPromotor(editando) {
    const titulo = document.getElementById('modalPromotorLabel');
    const btnSalvar = document.getElementById('btnSalvar');
    const btnSalvarTopo = document.getElementById('btnSalvarEmpresa');

    if (titulo) titulo.textContent = editando ? 'Editar Promotor' : 'Novo Promotor';
    if (btnSalvar) btnSalvar.textContent = editando ? 'Salvar alterações' : 'Cadastrar';
    if (btnSalvarTopo) btnSalvarTopo.textContent = editando ? 'Salvar alterações' : 'Salvar';
}

function preencherFormularioPromotor(promotor) {
    document.getElementById('promotorId').value = promotor.id || '';
    document.getElementById('nome').value = promotor.nome || '';
    document.getElementById('empresa').value = getEmpresaIdPrincipal(promotor);
    document.getElementById('cpf').value = aplicarMascaraCPF(promotor.cpf || '');
    document.getElementById('categoria').value = isPromotorExclusivo(promotor)
        ? 'Promotor Exclusivo'
        : 'Promotor';
    document.getElementById('telefone').value = aplicarMascaraTelefone(promotor.telefone || '');
    document.getElementById('email').value = normalizarEmail(promotor.email || '');
    setDiasPermitidosSelecionados(promotor.diasPermitidos);

    ['cpf', 'telefone', 'email', 'cepPromotor'].forEach(id => {
        limparFeedback(document.getElementById(id));
    });
}

async function carregarPromotores() {
    try {
        promotoresLista = await getPromotores();
        renderizarPromotores();
    } catch (error) {
        console.error('Erro ao carregar promotores:', error);
    }
}

function renderizarPromotores() {
    const container = document.getElementById('listaPromotores');
    if (!container) return;

    const filtro = normalizarBusca(document.getElementById('filtroPromotor')?.value);
    const filtroDigits = somenteDigitos(filtro);
    const promotoresFiltrados = promotoresLista.filter(promotor => {
        if (!filtro) return true;

        const empresaNome = formatarEmpresasPromotor(promotor);
        const campos = [
            promotor.nome,
            promotor.cpf,
            formatarCPFExibicao(promotor.cpf),
            promotor.email,
            empresaNome,
            promotor.tipo,
            formatarTipoPromotor(promotor.tipo)
        ].map(normalizarBusca);

        const cpfDigits = somenteDigitos(promotor.cpf);

        return campos.some(campo => campo.includes(filtro))
            || (!!filtroDigits && cpfDigits.includes(filtroDigits));
    });
    
    container.innerHTML = '';

    if (promotoresFiltrados.length === 0) {
        const mensagem = filtro
            ? 'Nenhum promotor encontrado'
            : 'Nenhum promotor cadastrado';
        const detalhe = filtro
            ? 'Revise o termo buscado ou limpe o filtro para ver todos os promotores.'
            : 'Cadastre um promotor para que ele apareça nesta lista.';

        container.innerHTML = `
            <div class="col-12">
                <div class="card">
                    <div class="card-body text-center py-5">
                        <div class="text-light fw-semibold mb-1">${mensagem}</div>
                        <small class="text-muted">${detalhe}</small>
                    </div>
                </div>
            </div>
        `;
        return;
    }
    
    promotoresFiltrados.forEach(promotor => {
        const col = document.createElement('div');
        col.className = 'col-md-6 col-lg-4';
        col.innerHTML = `
            <div class="card">
                <div class="card-body">
                    <h6 class="card-title">${promotor.nome}</h6>
                    <p class="card-text">
                        <small><strong>CPF:</strong> ${formatarCPFExibicao(promotor.cpf)}</small><br/>
                        <small><strong>Telefone:</strong> ${formatarTelefoneExibicao(promotor.telefone)}</small><br/>
                        <small><strong>Email:</strong> ${promotor.email || '-'}</small><br/>
                        <small><strong>Tipo:</strong> ${formatarTipoPromotor(promotor.tipo)}</small><br/>
                        <small><strong>Empresa:</strong> ${formatarEmpresasPromotor(promotor)}</small><br/>
                        <small><strong>Dias:</strong> ${formatarDiasPermitidos(promotor.diasPermitidos)}</small>
                    </p>
                    <div class="d-flex gap-2">
                        <button class="btn btn-sm btn-edit" onclick="editarPromotor(${promotor.id})">Editar</button>
                        <button class="btn btn-sm btn-delete" onclick="deletarPromotor(${promotor.id})">Deletar</button>
                    </div>
                </div>
            </div>
        `;
        container.appendChild(col);
    });
}

function limparFormularioPromotor() {
    document.getElementById('formPromotor').reset();
    document.getElementById('promotorId').value = '';
    document.getElementById('categoria').value = 'Promotor';
    setDiasPermitidosSelecionados(['segunda', 'ter\u00e7a', 'quarta', 'quinta', 'sexta']);
    setModoFormularioPromotor(false);
    ['cpf', 'telefone', 'email', 'cepPromotor'].forEach(id => {
        limparFeedback(document.getElementById(id));
    });
}

async function editarPromotor(id) {
    try {
        if (empresasPromotorLista.length === 0) {
            await carregarEmpresasSelectPromotor();
        }

        const promotor = await getPromotor(id);
        preencherFormularioPromotor(promotor);
        setModoFormularioPromotor(true);

        const modalEl = document.getElementById('modalPromotor');
        bootstrap.Modal.getOrCreateInstance(modalEl).show();
    } catch (error) {
        console.error('Erro ao editar:', error);
        alert('Erro ao carregar promotor para edição');
    }
}

async function deletarPromotor(id) {
    if (confirm('Tem certeza que deseja deletar este promotor?')) {
        try {
            await deletePromotor(id);
            await carregarPromotores();
        } catch (error) {
            console.error('Erro ao deletar:', error);
            alert('Erro ao deletar promotor');
        }
    }
}

function buscarCepPromotor() {
    const cepInput = document.getElementById('cepPromotor');
    const logradouro = document.getElementById('logradouroPromotor');
    const cidade = document.getElementById('cidadePromotor');
    const estado = document.getElementById('estadoPromotor');
    
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

function validarCPF(cpf) {
    cpf = cpf.replace(/[^\d]+/g, '');

    if (cpf.length !== 11) return false;
    if (/^(\d)\1+$/.test(cpf)) return false;

    let soma = 0, resto;

    for (let i = 1; i <= 9; i++) {
        soma += parseInt(cpf.substring(i - 1, i)) * (11 - i);
    }

    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(cpf.substring(9, 10))) return false;

    soma = 0;
    for (let i = 1; i <= 10; i++) {
        soma += parseInt(cpf.substring(i - 1, i)) * (12 - i);
    }

    resto = (soma * 10) % 11;
    if (resto === 10 || resto === 11) resto = 0;
    if (resto !== parseInt(cpf.substring(10, 11))) return false;

    return true;
}

function aplicarMascaraCPF(valor) {
    valor = somenteDigitos(valor, 11);

    valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
    valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
    valor = valor.replace(/(\d{3})(\d{1,2})$/, '$1-$2');

    return valor;
}

function validarCPFInput(input) {
    if (!input) return true;

    const cpf = input.value;
    const cpfDigits = somenteDigitos(cpf, 11);
    input.value = aplicarMascaraCPF(cpf);

    if (!cpfDigits) {
        return marcarCampoInvalido(input, 'Informe o CPF.');
    }

    return validarCPF(input.value)
        ? marcarCampoValido(input)
        : marcarCampoInvalido(input, 'CPF deve estar no formato 000.000.000-00 e ser valido.');
}

async function handleSalvarPromotor(e) {
    e.preventDefault();

    const cpfInput = document.getElementById("cpf");
    const telefoneInput = document.getElementById('telefone');
    const emailInput = document.getElementById('email');

    if (!validarCPFInput(cpfInput)) {
        cpfInput.focus();
        return;
    }

    if (!validarTelefoneInput(telefoneInput, false)) {
        telefoneInput.focus();
        return;
    }

    if (!validarEmailInput(emailInput, false)) {
        emailInput.focus();
        return;
    }

    const id = document.getElementById('promotorId').value;
    const empresaId = parseInt(document.getElementById('empresa').value, 10);

    if (Number.isNaN(empresaId)) {
        alert('Selecione uma empresa');
        return;
    }

    const cpfFormatado = aplicarMascaraCPF(cpfInput.value);
    cpfInput.value = cpfFormatado;

    const data = {
        nome: document.getElementById('nome').value.trim(),
        cpf: cpfFormatado,
        telefone: telefoneInput.value.trim(),
        email: emailInput.value.trim(),
        tipo: getTipoSelecionado(),
        empresaId,
        diasPermitidos: getDiasPermitidosSelecionados()
    };

    try {
        if (id) {
            await updatePromotor(id, data);
        } else {
            await createPromotor(data);
        }

        await carregarPromotores();

        bootstrap.Modal.getInstance(document.getElementById('modalPromotor'))?.hide();

        limparFormularioPromotor();
    } catch (e) {
        console.error('Erro ao salvar:', e);
        alert('Erro ao salvar promotor');
    }
}

window.editarPromotor = editarPromotor;
window.deletarPromotor = deletarPromotor;
