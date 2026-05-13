// promotores.js - Gerencia CRUD de promotores

let promotoresLista = [];
let empresasPromotorLista = [];

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
    
    // Configurar event listeners para CEP
    const btnBuscarCep = document.getElementById('btnBuscarCep');
    if (btnBuscarCep) {
        btnBuscarCep.addEventListener('click', buscarCepPromotor);
    }
    
    const cepInput = document.getElementById('cep');
    if (cepInput) {
        cepInput.addEventListener('blur', buscarCepPromotor);
        cepInput.addEventListener('input', () => {
            let cep = cepInput.value.replace(/\D/g, '');
            if (cep.length === 8) {
                buscarCepPromotor();
            }
        });
    }
});

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
    document.getElementById('telefone').value = promotor.telefone || '';
    document.getElementById('email').value = promotor.email || '';
    setDiasPermitidosSelecionados(promotor.diasPermitidos);

    document.getElementById('cpf').classList.remove('erro', 'sucesso');
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
    
    container.innerHTML = '';
    
    promotoresLista.forEach(promotor => {
        const col = document.createElement('div');
        col.className = 'col-md-6 col-lg-4';
        col.innerHTML = `
            <div class="card">
                <div class="card-body">
                    <h6 class="card-title">${promotor.nome}</h6>
                    <p class="card-text">
                        <small><strong>CPF:</strong> ${promotor.cpf}</small><br/>
                        <small><strong>Telefone:</strong> ${promotor.telefone || '-'}</small><br/>
                        <small><strong>Email:</strong> ${promotor.email || '-'}</small><br/>
                        <small><strong>Tipo:</strong> ${formatarTipoPromotor(promotor.tipo)}</small><br/>
                        <small><strong>Empresa:</strong> ${formatarEmpresasPromotor(promotor)}</small><br/>
                        <small><strong>Dias:</strong> ${formatarDiasPermitidos(promotor.diasPermitidos)}</small>
                    </p>
                    <div class="d-flex gap-2">
                        <button class="btn btn-sm btn-outline-primary" onclick="editarPromotor(${promotor.id})">Editar</button>
                        <button class="btn btn-sm btn-outline-secondary" onclick="deletarPromotor(${promotor.id})">Deletar</button>
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
    document.getElementById('cpf').classList.remove('erro', 'sucesso');
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
    
    let cep = cepInput.value.replace(/\D/g, '');

    cepInput.classList.remove('erro', 'sucesso');

    if (cep.length !== 8) {
        cepInput.classList.add('erro');
        alert('CEP inválido');
        return;
    }

    logradouro.classList.add('loading');
    logradouro.value = 'Buscando...';

    fetch(`https://viacep.com.br/ws/${cep}/json/`)
        .then(res => res.json())
        .then(data => {
            logradouro.classList.remove('loading');

            if (data.erro) {
                cepInput.classList.add('erro');
                logradouro.value = '';
                alert('CEP não encontrado');
                return;
            }

            logradouro.value = data.logradouro;
            cidade.value = data.localidade;
            estado.value = data.uf;
            cepInput.classList.add('sucesso');
        })
        .catch(() => {
            logradouro.classList.remove('loading');
            logradouro.value = '';
            cepInput.classList.add('erro');
            alert('Erro ao buscar CEP');
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
    valor = valor.replace(/\D/g, '');

    valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
    valor = valor.replace(/(\d{3})(\d)/, '$1.$2');
    valor = valor.replace(/(\d{3})(\d{1,2})$/, '$1-$2');

    return valor;
}

const cpfInput = document.getElementById("cpf");

cpfInput?.addEventListener("input", () => {
    cpfInput.value = aplicarMascaraCPF(cpfInput.value);
    cpfInput.classList.remove("erro", "sucesso");
});

cpfInput?.addEventListener("blur", () => {
    const cpf = cpfInput.value;

    if (!cpf) return;

    if (!validarCPF(cpf)) {
        cpfInput.classList.add("erro");
    } else {
        cpfInput.classList.add("sucesso");
    }
});

async function handleSalvarPromotor(e) {
    e.preventDefault();

    const cpfInput = document.getElementById("cpf");
    const cpf = cpfInput.value;
    const cpfDigits = cpf.replace(/\D/g, '');

    // O backend valida o formato 000.000.000-00.
    if (cpfDigits.length !== 11) {
        cpfInput.classList.add("erro");
        cpfInput.focus();
        return;
    }

    const id = document.getElementById('promotorId').value;
    const empresaId = parseInt(document.getElementById('empresa').value, 10);

    if (Number.isNaN(empresaId)) {
        alert('Selecione uma empresa');
        return;
    }

    const cpfFormatado = aplicarMascaraCPF(cpf);
    cpfInput.value = cpfFormatado;
    cpfInput.classList.remove("erro");

    const data = {
        nome: document.getElementById('nome').value.trim(),
        cpf: cpfFormatado,
        telefone: document.getElementById('telefone').value.trim(),
        email: document.getElementById('email').value.trim(),
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
