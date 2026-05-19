// promotores.js - Gerencia CRUD de promotores

let promotoresLista = [];

document.addEventListener('DOMContentLoaded', async function() {
    // Verifica autenticação
    requireAuth();
    
    // Carrega empresas para select
    await carregarEmpresas();
    
    // Carrega promotores
    await carregarPromotores();
    
    // Listeners de formulário
    const formPromotor = document.getElementById('formPromotor');
    if (formPromotor) {
        formPromotor.addEventListener('submit', handleSalvarPromotor);
    }
    
    const btnNovoPromotor = document.getElementById('btnNovoPromotor');
    if (btnNovoPromotor) {
        btnNovoPromotor.addEventListener('click', limparFormulario);
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

async function carregarEmpresas() {
    try {
        const empresas = await getEmpresas();
        const selectEmpresa = document.getElementById('empresa');
        if (selectEmpresa) {
            empresas.forEach(emp => {
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
                        <small><strong>Email:</strong> ${promotor.email || '-'}</small>
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

async function handleSalvarPromotor(e) {
    e.preventDefault();
    
    const id = document.getElementById('promotorId').value;
    const nome = document.getElementById('nome').value;
    const empresa = document.getElementById('empresa').value;
    const cpf = document.getElementById('cpf').value;
    
    const data = {
        nome,
        cpf,
        telefone: document.getElementById('telefone').value,
        email: document.getElementById('email').value,
        empresaId: parseInt(empresa)
    };
    
    try {
        if (id) {
            await updatePromotor(id, data);
        } else {
            await createPromotor(data);
        }
        
        // Recarrega lista
        await carregarPromotores();
        
        // Fecha modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('modalPromotor'));
        if (modal) modal.hide();
        
        limparFormulario();
    } catch (error) {
        console.error('Erro ao salvar:', error);
        alert('Erro ao salvar promotor');
    }
}

function limparFormulario() {
    document.getElementById('formPromotor').reset();
    document.getElementById('promotorId').value = '';
}

async function editarPromotor(id) {
    try {
        const promotor = await getPromotor(id);
        document.getElementById('promotorId').value = promotor.id;
        document.getElementById('nome').value = promotor.nome;
        document.getElementById('empresa').value = promotor.empresaId;
        document.getElementById('cpf').value = promotor.cpf;
        document.getElementById('telefone').value = promotor.telefone || '';
        document.getElementById('email').value = promotor.email || '';
        
        const modal = new bootstrap.Modal(document.getElementById('modalPromotor'));
        modal.show();
    } catch (error) {
        console.error('Erro ao editar:', error);
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

function buscarCep() {
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

    // 🚨 valida antes de salvar
    if (!validarCPF(cpf)) {
        cpfInput.classList.add("erro");
        cpfInput.focus();
        return;
    }

    const id = document.getElementById('promotorId').value;

    const data = {
        nome: document.getElementById('nome').value,
        cpf: cpf.replace(/\D/g, ''), // salva limpo
        telefone: document.getElementById('telefone').value,
        email: document.getElementById('email').value,
        empresaId: parseInt(document.getElementById('empresa').value)
    };

    try {
        if (id) {
            await updatePromotor(id, data);
        } else {
            await createPromotor(data);
        }

        await carregarPromotores();

        bootstrap.Modal.getInstance(document.getElementById('modalPromotor'))?.hide();

        limparFormulario();
    } catch (e) {
        console.error('Erro ao salvar:', e);
        alert('Erro ao salvar promotor');
    }
}