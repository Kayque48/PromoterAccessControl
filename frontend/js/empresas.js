// empresas.js - Gerencia CRUD de empresas

let empresasLista = [];

document.addEventListener("DOMContentLoaded", async function () {
  requireAuth();
  await carregarEmpresas();

  // Máscaras
  
  const cnpjEl = document.getElementById("cnpj");
  const cepEl = document.getElementById("cepEmpresa");
  const telefoneEl = document.getElementById("telefoneEmpresa");

  f (cnpjEl) {
    cnpjEl.addEventListener("input", (e) => {
      e.target.value = mascaraCNPJ(e.target.value);
      // Dispara busca ao completar 14 dígitos
      if (e.target.value.replace(/\D/g, "").length === 14) buscarCNPJ();
    });
  }

  if (cepEl) {
    cepEl.addEventListener("input", (e) => {
      e.target.value = mascaraCEP(e.target.value);
      if (e.target.value.replace(/\D/g, "").length === 8) buscarCep();
    });
    cepEl.addEventListener("blur", buscarCep);
  }

  if (telefoneEl) {
    telefoneEl.addEventListener("input", (e) => {
      e.target.value = mascaraTelefone(e.target.value);
    });
  }

  const formEmpresa = document.getElementById("formEmpresa");
  if (formEmpresa) {
    formEmpresa.addEventListener("submit", handleSalvarEmpresa);
  }

  const btnCancelar = document.getElementById("btnCancelarEmpresa");
  if (btnCancelar) {
    btnCancelar.addEventListener("click", limparFormulario);
  }

  // (Listeners de CEP/CNPJ são configurados no escopo global para compatibilidade
  // com implementações anteriores e com formulários carregados estaticamente.)
});

async function carregarEmpresas() {
  try {
    empresasLista = await getEmpresas();
    renderizarEmpresas();
  } catch (error) {
    console.error("Erro ao carregar empresas:", error);
  }
}

function renderizarEmpresas() {
  const tbody = document.getElementById("tbodyEmpresas");
  if (!tbody) return;

  tbody.innerHTML = "";

  empresasLista.forEach((empresa) => {
    const tr = document.createElement("tr");
    tr.innerHTML = `
            <td>${empresa.cnpj}</td>
            <td>${empresa.razaoSocial}</td>
            <td>${empresa.nomeFantasia || "-"}</td>
            <td>${empresa.telefone || "-"}</td>
            <td>${empresa.email || "-"}</td>
            <td>${empresa.endereco || "-"}</td>
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

  const id = document.getElementById("empresaId").value;
  const logradouro = document.getElementById("logradouroEmpresa").value;
  const numero = document.getElementById("numeroEmpresa").value;
  const complemento = document.getElementById("complementoEmpresa").value;
  const enderecoPartes = [logradouro, numero, complemento].filter(Boolean);

  const data = {
    cnpj: document.getElementById("cnpj").value.replace(/\D/g, ""),
    razaoSocial: document.getElementById("razaoSocial").value,
    nomeFantasia: document.getElementById("nomeFantasia").value,
    telefone: document.getElementById("telefoneEmpresa").value,
    email: document.getElementById("emailEmpresa").value,
    endereco: enderecoPartes.join(", "),
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
    console.error("Erro ao salvar:", error);
    alert("Erro ao salvar empresa");
  }
}

function limparFormulario() {
  document.getElementById("formEmpresa").reset();
  document.getElementById("empresaId").value = "";
}

async function editarEmpresa(id) {
  try {
    const empresa = await getEmpresa(id);
    document.getElementById("empresaId").value = empresa.id;
    document.getElementById("cnpj").value = empresa.cnpj;
    document.getElementById("razaoSocial").value = empresa.razaoSocial;
    document.getElementById("nomeFantasia").value = empresa.nomeFantasia || "";
    document.getElementById("telefoneEmpresa").value = empresa.telefone || "";
    document.getElementById("emailEmpresa").value = empresa.email || "";
    document.getElementById("logradouroEmpresa").value = empresa.endereco || "";
    document.getElementById("numeroEmpresa").value = "";
    document.getElementById("complementoEmpresa").value = "";
  } catch (error) {
    console.error("Erro ao editar:", error);
  }
}

async function deletarEmpresa(id) {
  if (confirm("Tem certeza que deseja deletar esta empresa?")) {
    try {
      await deleteEmpresa(id);
      await carregarEmpresas();
    } catch (error) {
      console.error("Erro ao deletar:", error);
      alert("Erro ao deletar empresa");
    }
  }
}
function buscarCep() {
  const cepInput = document.getElementById("cepEmpresa");
  const logradouro = document.getElementById("logradouroEmpresa");
  const cidade = document.getElementById("cidadeEmpresa");
  const estado = document.getElementById("estadoEmpresa");

  let cep = cepInput.value.replace(/\D/g, "");

  cepInput.classList.remove("erro", "sucesso");

  if (cep.length !== 8) {
    cepInput.classList.add("erro");
    alert("CEP inválido");
    return;
  }

  logradouro.classList.add("loading");
  logradouro.value = "Buscando...";

  fetch(`https://viacep.com.br/ws/${cep}/json/`)
    .then((res) => res.json())
    .then((data) => {
      logradouro.classList.remove("loading");

      if (data.erro) {
        cepInput.classList.add("erro");
        logradouro.value = "";
        alert("CEP não encontrado");
        return;
      }

      logradouro.value = data.logradouro;
      cidade.value = data.localidade;
      estado.value = data.uf;
      cepInput.classList.add("sucesso");
    })
    .catch(() => {
      logradouro.classList.remove("loading");
      logradouro.value = "";
      cepInput.classList.add("erro");
      alert("Erro ao buscar CEP");
    });
}

function buscarCNPJ() {
  const cnpjInput = document.getElementById("cnpj");
  const razaoSocial = document.getElementById("razaoSocial");

  let cnpj = cnpjInput.value.replace(/\D/g, "");

  cnpjInput.classList.remove("erro", "sucesso");

  if (cnpj.length !== 14) {
    cnpjInput.classList.add("erro");
    alert("CNPJ inválido");
    return;
  }

  razaoSocial.classList.add("loading");
  razaoSocial.value = "Buscando...";

  fetch(`https://brasilapi.com.br/api/cnpj/v1/${cnpj}`)
    .then((res) => {
      if (!res.ok) throw new Error("Não encontrado");
      return res.json();
    })
    .then((data) => {
      razaoSocial.classList.remove("loading");

      if (!data || data.message || data.status === "ERROR") {
        cnpjInput.classList.add("erro");
        razaoSocial.value = "";
        alert("CNPJ não encontrado");
        return;
      }

      // Preenche campos disponíveis
      razaoSocial.value = data.razao_social || data.nome || "";
      const nomeFantasiaEl = document.getElementById("nomeFantasia");
      if (nomeFantasiaEl) nomeFantasiaEl.value = data.nome_fantasia || "";

      // Alguns serviços retornam endereço em objeto diferente
      try {
        const logradouroEl = document.getElementById("logradouroEmpresa");
        const cidadeEl = document.getElementById("cidadeEmpresa");
        const estadoEl = document.getElementById("estadoEmpresa");

        if (data.estabelecimento) {
          if (logradouroEl && data.estabelecimento.logradouro)
            logradouroEl.value = data.estabelecimento.logradouro;
          if (cidadeEl && data.estabelecimento.municipio)
            cidadeEl.value = data.estabelecimento.municipio;
          if (estadoEl && data.estabelecimento.uf)
            estadoEl.value = data.estabelecimento.uf;
        }
      } catch (e) {
        // ignore
      }

      cnpjInput.classList.add("sucesso");
    })
    .catch(() => {
      razaoSocial.classList.remove("loading");
      razaoSocial.value = "";
      cnpjInput.classList.add("erro");
      alert("Erro ao buscar CNPJ");
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

//-- Máscara de Formatação -- //

function mascaraCPNJ(valor) {
  return valor
    .replace(/\D/g, "")
    .substring(0, 14)
    .replace(/^(\d{2})(\d)/, "$1.$2")
    .replace(/^(\d{2})\.(\d{3})(\d)/, "$1.$2.$3")
    .replace(/\.(\d{3})(\d)/, ".$1/$2")
    .replace(/(\d{4})(\d)/, "$1-$2");
}

function mascaraCEP(valor) {
  return valor
    .replace(/\D/g, "")
    .substring(0, 8)
    .replace(/^(\d{5})(\d)/, "$1-$2");
}

function mascaraTelefone(valor) {
  const digits = valor.replace(/\D/g, "").substring(0, 11);
  if (digits.length <= 10) {
    // Fixo: (11) 1234-5678
    return digits
      .replace(/^(\d{2})(\d)/, "($1) $2")
      .replace(/(\d{4})(\d)/, "$1-$2");
  }
  // Celular: (11) 91234-5678
  return digits
    .replace(/^(\d{2})(\d)/, "($1) $2")
    .replace(/(\d{5})(\d)/, "$1-$2");
}
