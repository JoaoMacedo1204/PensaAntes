/**
 * desafios.js: lógica da página de listagem de desafios.
 *
 * Fluxo: conferir quem é o usuário, buscar os desafios na API e mostrar
 * um (e só um) destes estados: carregando, lista, vazio ou erro.
 *
 * A identificação por e-mail é uma solução provisória do MVP, não é login.
 */

import { listarDesafios, ApiError, TIPO_ERRO } from "../api.js";
import { recuperarUsuario, removerUsuario } from "../armazenamento.js";

const PAGINA_HOME = "/index.html";
const PAGINA_DESAFIO = "/desafio.html";

// Chave: nome do nível sem acento e em minúsculas. Valor: número de pontos e texto exibido.
const NIVEIS = {
  facil: { numero: 1, rotulo: "Fácil" },
  medio: { numero: 2, rotulo: "Médio" },
  dificil: { numero: 3, rotulo: "Difícil" },
};

const saudacao = document.getElementById("saudacao");
const linkTrocarUsuario = document.getElementById("link-trocar-usuario");
const areaDesafios = document.getElementById("area-desafios");
const estadoCarregando = document.getElementById("estado-carregando");
const estadoVazio = document.getElementById("estado-vazio");
const estadoErro = document.getElementById("estado-erro");
const mensagemErro = document.getElementById("mensagem-erro");
const botaoTentarNovamente = document.getElementById("botao-tentar-novamente");
const listaDesafios = document.getElementById("lista-desafios");
const modeloCartao = document.getElementById("modelo-cartao-desafio");

function iniciar(usuario) {
  let buscando = false;

  // --- Tela -----------------------------------------------------------------

  // Mostra só o estado pedido: "carregando", "vazio", "erro" ou "lista".
  function mostrarEstado(estado) {
    estadoCarregando.hidden = estado !== "carregando";
    estadoVazio.hidden = estado !== "vazio";
    estadoErro.hidden = estado !== "erro";
    listaDesafios.hidden = estado !== "lista";

    // aria-busy="true" avisa leitores de tela que a região ainda está sendo atualizada.
    areaDesafios.setAttribute("aria-busy", String(estado === "carregando"));
  }

  function primeiroNome(nome) {
    return nome.trim().split(/\s+/)[0];
  }

  // --- Cartões --------------------------------------------------------------

  // "Fácil", "facil" e "FACIL" viram "facil": tira acentos e maiúsculas.
  function chaveDoNivel(texto) {
    return String(texto ?? "")
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "")
      .trim()
      .toLowerCase();
  }

  function preencherNivel(cartao, dificuldade) {
    const nivel = cartao.querySelector(".nivel");
    const texto = cartao.querySelector(".nivel__texto");
    const conhecido = NIVEIS[chaveDoNivel(dificuldade)];

    if (conhecido) {
      nivel.classList.add(`nivel--${conhecido.numero}`);
      texto.textContent = conhecido.rotulo;
    } else {
      // Nível desconhecido: mantém o texto original e tira os pontos, que não teriam o que indicar.
      texto.textContent = String(dificuldade ?? "");
      cartao.querySelector(".nivel__pontos").remove();
    }
  }

  // Devolve o cartão pronto, ou null se o desafio não tem um id válido para o link.
  function criarCartao(desafio) {
    const id = Number(desafio?.id);

    if (!Number.isInteger(id) || id <= 0) {
      console.warn("[desafios] desafio ignorado, id inválido:", desafio);
      return null;
    }

    // cloneNode(true) copia o conteúdo do template, inclusive o que está dentro dos elementos.
    const copia = modeloCartao.content.cloneNode(true);
    const cartao = copia.firstElementChild;

    // textContent: o texto da API nunca é lido como HTML.
    cartao.querySelector(".cartao-desafio__categoria").textContent = String(desafio.categoria ?? "");
    cartao.querySelector(".cartao-desafio__titulo").textContent = String(desafio.titulo ?? "");
    cartao.querySelector(".cartao-desafio__descricao").textContent = String(desafio.descricao ?? "");
    cartao.querySelector(".cartao-desafio__link-titulo").textContent = String(desafio.titulo ?? "");
    cartao.querySelector(".cartao-desafio__link").setAttribute("href", `${PAGINA_DESAFIO}?id=${id}`);

    preencherNivel(cartao, desafio.dificuldade);

    return cartao;
  }

  function mostrarLista(desafios) {
    const cartoes = document.createDocumentFragment();

    for (const desafio of desafios) {
      const cartao = criarCartao(desafio);
      if (cartao) cartoes.appendChild(cartao);
    }

    listaDesafios.replaceChildren(); // limpa a lista, para não repetir cartões em "Tentar de novo"

    if (!cartoes.hasChildNodes()) {
      mostrarEstado("vazio");
      return;
    }

    listaDesafios.appendChild(cartoes);
    mostrarEstado("lista");
  }

  // --- API ------------------------------------------------------------------

  function mensagemParaErro(erro) {
    if (erro instanceof ApiError) {
      return erro.message; // frase própria do api.js, segura para exibir
    }

    console.error("[desafios] erro inesperado:", erro);
    return "Algo deu errado. Tente de novo.";
  }

  async function carregar() {
    if (buscando) return; // já tem uma busca em andamento

    buscando = true;
    mostrarEstado("carregando");

    try {
      const desafios = await listarDesafios();

      if (!Array.isArray(desafios)) {
        // 200, mas sem o array esperado (por exemplo, uma página HTML no lugar do JSON).
        throw new ApiError(TIPO_ERRO.SERVIDOR, { detalheTecnico: "resposta sem lista de desafios" });
      }

      mostrarLista(desafios);
    } catch (erro) {
      mensagemErro.textContent = mensagemParaErro(erro);
      mostrarEstado("erro");
    } finally {
      buscando = false;
    }
  }

  // --- Eventos --------------------------------------------------------------

  saudacao.textContent = `Olá, ${primeiroNome(usuario.nome)}!`;

  // Sem preventDefault: o link segue para a Home depois de limpar a identificação.
  linkTrocarUsuario.addEventListener("click", () => {
    removerUsuario();
  });

  botaoTentarNovamente.addEventListener("click", carregar);

  // Ao voltar pelo botão "voltar", o navegador pode reaproveitar a página como estava.
  // Se a identificação foi removida nesse meio tempo, a pessoa volta para a Home.
  window.addEventListener("pageshow", (evento) => {
    if (evento.persisted && !recuperarUsuario()) {
      window.location.replace(PAGINA_HOME);
    }
  });

  carregar();
}

const elementosPresentes = [
  saudacao,
  linkTrocarUsuario,
  areaDesafios,
  estadoCarregando,
  estadoVazio,
  estadoErro,
  mensagemErro,
  botaoTentarNovamente,
  listaDesafios,
  modeloCartao,
].every(Boolean);

if (elementosPresentes) {
  const usuario = recuperarUsuario();

  if (usuario) {
    iniciar(usuario);
  } else {
    // Sem identificação: volta para a Home (replace não deixa a página no histórico).
    window.location.replace(PAGINA_HOME);
  }
}