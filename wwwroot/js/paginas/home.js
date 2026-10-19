/**
 * home.js: lógica do formulário de identificação da Home.
 *
 * Fluxo: validar os campos, tentar cadastrar; se o e-mail já existir (409),
 * recuperar o usuário existente; salvar a identificação no navegador e ir
 * para a lista de desafios.
 *
 * A identificação por e-mail é uma solução provisória do MVP, não é login.
 */

import {
  cadastrarUsuario,
  buscarUsuarioPorEmail,
  ApiError,
  TIPO_ERRO,
} from "../api.js";
import { salvarUsuario } from "../armazenamento.js";

const PAGINA_DESAFIOS = "/desafios.html";
const TEXTO_CARREGANDO = "Começando…";

// Limites iguais aos do backend (CriarUsuarioDto).
const LIMITE_NOME = 100;
const LIMITE_EMAIL = 254;
const LIMITE_AREA = 100;

// Bem permissivo, como o do backend: um "@" com texto dos dois lados e sem espaços.
// Quem decide de verdade é o servidor.
const EMAIL_VALIDO = /^[^\s@]+@[^\s@]+$/;

const formulario = document.getElementById("form-identificacao");
const mensagemForm = document.getElementById("mensagem-form");
const botao = formulario?.querySelector('button[type="submit"]');

function iniciar() {
  const textoOriginal = botao.textContent;
  let enviando = false;

  // Desliga os avisos nativos do navegador para mostrarmos as nossas mensagens.
  formulario.noValidate = true;

  // --- Tela -----------------------------------------------------------------

  function mostrarMensagem(texto) {
    mensagemForm.textContent = texto; // textContent: o texto nunca é lido como HTML
  }

  function marcarCampo(nome) {
    formulario.elements[nome]?.setAttribute("aria-invalid", "true");
  }

  function limparErros() {
    mensagemForm.textContent = "";
    for (const campo of formulario.querySelectorAll('[aria-invalid="true"]')) {
      campo.removeAttribute("aria-invalid");
    }
  }

  function definirCarregando(carregando) {
    botao.disabled = carregando;
    botao.textContent = carregando ? TEXTO_CARREGANDO : textoOriginal;
    formulario.setAttribute("aria-busy", String(carregando));
  }

  // Volta o formulário ao normal e mostra o motivo da falha.
  function terminarComErro(mensagem, camposInvalidos = []) {
    enviando = false;
    definirCarregando(false);
    mostrarMensagem(mensagem);
    camposInvalidos.forEach(marcarCampo);
  }

  // --- Dados ----------------------------------------------------------------

  function lerDados() {
    const area = formulario.elements.areaPreferencia.value.trim();

    return {
      nome: formulario.elements.nome.value.trim(),
      email: formulario.elements.email.value.trim(),
      areaPreferencia: area === "" ? undefined : area,
    };
  }

  // Devolve a lista de problemas: [{ campo, mensagem }]. Lista vazia = tudo certo.
  function validar(dados) {
    const problemas = [];

    if (dados.nome === "") {
      problemas.push({ campo: "nome", mensagem: "Informe seu nome." });
    } else if (dados.nome.length > LIMITE_NOME) {
      problemas.push({
        campo: "nome",
        mensagem: `O nome pode ter até ${LIMITE_NOME} caracteres.`,
      });
    }

    if (dados.email === "") {
      problemas.push({ campo: "email", mensagem: "Informe seu e-mail." });
    } else if (dados.email.length > LIMITE_EMAIL || !EMAIL_VALIDO.test(dados.email)) {
      problemas.push({
        campo: "email",
        mensagem: "Informe um e-mail válido, como nome@exemplo.com.",
      });
    }

    if ((dados.areaPreferencia?.length ?? 0) > LIMITE_AREA) {
      problemas.push({
        campo: "areaPreferencia",
        mensagem: `A área de interesse pode ter até ${LIMITE_AREA} caracteres.`,
      });
    }

    return problemas;
  }

  // --- API ------------------------------------------------------------------

  // Tenta cadastrar. Se o e-mail já existe (409), recupera o usuário que já está cadastrado.
  async function identificarUsuario(dados) {
    try {
      return await cadastrarUsuario(dados);
    } catch (erro) {
      if (erro instanceof ApiError && erro.tipo === TIPO_ERRO.EMAIL_JA_CADASTRADO) {
        return buscarUsuarioPorEmail(dados.email);
      }
      throw erro;
    }
  }

  function mensagemParaErro(erro) {
    if (erro instanceof ApiError) {
      if (erro.tipo === TIPO_ERRO.NAO_ENCONTRADO) {
        // Só chega aqui se o e-mail estava cadastrado (409) e sumiu na busca seguinte.
        return "Este e-mail já está cadastrado, mas não conseguimos recuperar seu cadastro agora. Tente de novo.";
      }
      return erro.message; // frase própria do api.js, segura para exibir
    }

    console.error("[home] erro inesperado:", erro);
    return "Algo deu errado. Tente de novo.";
  }

  // --- Eventos --------------------------------------------------------------

  formulario.addEventListener("submit", async (evento) => {
    evento.preventDefault(); // impede o navegador de recarregar a página com os dados na URL

    if (enviando) return;

    limparErros();

    const dados = lerDados();
    const problemas = validar(dados);

    if (problemas.length > 0) {
      problemas.forEach((problema) => marcarCampo(problema.campo));
      mostrarMensagem(problemas.map((problema) => problema.mensagem).join(" "));
      formulario.elements[problemas[0].campo].focus();
      return;
    }

    enviando = true;
    definirCarregando(true);

    try {
      const usuario = await identificarUsuario(dados);

      if (!salvarUsuario(usuario)) {
        terminarComErro(
          "Não foi possível salvar sua identificação neste navegador. Verifique as configurações de privacidade e tente novamente."
        );
        return;
      }

      // O botão continua desabilitado enquanto a próxima página carrega.
      window.location.assign(PAGINA_DESAFIOS);
    } catch (erro) {
      const campos = erro instanceof ApiError ? erro.campos : [];
      terminarComErro(mensagemParaErro(erro), campos);
    }
  });

  // Ao voltar pelo botão "voltar", o navegador pode reaproveitar a página como ela estava
  // (com o botão desabilitado). Aqui devolvemos o formulário ao normal.
  window.addEventListener("pageshow", (evento) => {
    if (evento.persisted) {
      enviando = false;
      definirCarregando(false);
    }
  });
}

if (formulario && mensagemForm && botao) {
  iniciar();
}