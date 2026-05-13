// empresas.js - Gerencia CRUD de empresas

let empresasLista = [];

document.addEventListener('DOMContentLoaded', async function() {
    requireAuth();
    await carregarEmpresas();
    
    const formEmpresa = document.getElementById('formEmpresa');
    if (formEmpresa) {
        formEmpresa.addEventListener('submit', handleSalvarEmpresa);
    }
    
    const btnCancelar = document.getElementById('btnCancelarEmpresa');
    if (btnCancelar) {
        btnCancelar.addEventListener('click', limparFormulario);
    }
    
    // (Listeners de CEP/CNPJ são configurados no escopo global para compatibilidade
    // com implementações anteriores e com formulários carregados estaticamente.)
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

function formatarCNPJ(cnpj) {
    const digits = cnpj.replace(/\D/g, '');
    if (digits.length !== 14) return cnpj.trim();

    return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5, 8)}/${digits.slice(8, 12)}-${digits.slice(12)}`;
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
    
    tbody.innerHTML = '';
    
    empresasLista.forEach(empresa => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${empresa.cnpj}</td>
            <td>${empresa.razaoSocial}</td>
            <td>${empresa.nomeFantasia || '-'}</td>
            <td>${empresa.telefone || '-'}</td>
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
    const endereco = montarEnderecoConsolidado();

    if (!endereco) {
        alert('Informe o endereço da empresa');
        return;
    }

    const data = {
        cnpj: formatarCNPJ(getInputValue('cnpj')),
        razaoSocial: getInputValue('razaoSocial'),
        nomeFantasia: getInputValue('nomeFantasia'),
        telefone: getInputValue('telefoneEmpresa'),
        email: getInputValue('emailEmpresa'),
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
}

async function editarEmpresa(id) {
    try {
        const empresa = await getEmpresa(id);
        document.getElementById('empresaId').value = empresa.id;
        document.getElementById('cnpj').value = empresa.cnpj;
        document.getElementById('razaoSocial').value = empresa.razaoSocial;
        document.getElementById('nomeFantasia').value = empresa.nomeFantasia || '';
        document.getElementById('telefoneEmpresa').value = empresa.telefone || '';
        document.getElementById('emailEmpresa').value = empresa.email || '';
        document.getElementById('logradouroEmpresa').value = empresa.endereco || '';
        document.getElementById('numeroEmpresa').value = '';
        document.getElementById('cidadeEmpresa').value = '';
        document.getElementById('estadoEmpresa').value = '';
        document.getElementById('complementoEmpresa').value = '';
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

function buscarCNPJ() {
    const cnpjInput = document.getElementById('cnpj');
    const razaoSocial = document.getElementById('razaoSocial');
    
    let cnpj = cnpjInput.value.replace(/\D/g, '');

    cnpjInput.classList.remove('erro', 'sucesso');

    if (cnpj.length !== 14) {
        cnpjInput.classList.add('erro');
        alert('CNPJ inválido');
        return;
    }

    razaoSocial.classList.add('loading');
    razaoSocial.value = 'Buscando...';

    fetch(`https://brasilapi.com.br/api/cnpj/v1/${cnpj}`)
        .then(res => {
            if (!res.ok) throw new Error('Não encontrado');
            return res.json();
        })
        .then(data => {
            razaoSocial.classList.remove('loading');

            if (!data || data.message || data.status === 'ERROR') {
                cnpjInput.classList.add('erro');
                razaoSocial.value = '';
                alert('CNPJ não encontrado');
                return;
            }

            // Preenche campos disponíveis
            razaoSocial.value = data.razao_social || data.nome || '';
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

            cnpjInput.classList.add('sucesso');
        })
        .catch(() => {
            razaoSocial.classList.remove('loading');
            razaoSocial.value = '';
            cnpjInput.classList.add('erro');
            alert('Erro ao buscar CNPJ');
        });
}

// --- Restaurar listeners globais (comportamento histórico) ---
const cepInput = document.getElementById("cepEmpresa");
const btnBuscar = document.getElementById("btnBuscarCepEmpresa");
const cidade = document.getElementById("cidadeEmpresa");
const estado = document.getElementById("estadoEmpresa");
const logradouro = document.getElementById("logradouroEmpresa");

if (btnBuscar) btnBuscar.addEventListener("click", buscarCep);

if (cepInput) {
    cepInput.addEventListener("blur", buscarCep);
    cepInput.addEventListener("input", () => {
        let cep = cepInput.value.replace(/\D/g, "");
        if (cep.length === 8) buscarCep();
    });
}

const cnpjInput = document.getElementById("cnpj");
if (cnpjInput) {
    cnpjInput.addEventListener("blur", buscarCNPJ);
    cnpjInput.addEventListener("input", () => {
        let cnpj = cnpjInput.value.replace(/\D/g, "");
        if (cnpj.length === 14) buscarCNPJ();
    });
}
