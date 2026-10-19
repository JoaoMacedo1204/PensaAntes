/**
 * desafio.js: lógica da página de resolução de um desafio.
 *
 * Fluxo: conferir quem é o usuário, ler o id da URL (?id=N), buscar o desafio
 * na API e mostrar um (e só um) destes estados: carregando, conteúdo,
 * não encontrado ou erro.
 *
 * Por enquanto o formulário só é bloqueado: salvar a resposta vem em outra etapa.
 * A identificação por e-mail é uma solução provisória do MVP, não é login.
 */

import { buscarDesafioPorId, criarRespostaInicial, consultarAnaliseIa, finalizarResposta, avaliarResposta, criarAutoavaliacao, buscarAutoavaliacao, ApiError, TIPO_ERRO } from "../api.js";
import { recuperarUsuario } from "../armazenamento.js";

const PAGINA_HOME = "/index.html";

// Resposta salva nesta aba do navegador: { respostaId, usuarioId, desafioId, analiseInicial, justificativaInicial }.
const CHAVE_RESPOSTA_SALVA = "pensaAntes.respostaAtual";

// Chave antiga, só com o id. Continua sendo gravada por compatibilidade,
// mas nunca é usada para restaurar nada: um id sozinho não diz de quem nem de qual desafio é.
const CHAVE_RESPOSTA_ATUAL = "pensaAntes.respostaAtualId";

const LIMITE_TEXTO = 5000; // mesmo limite do backend (CriarRespostaDto)
const TEXTO_SALVANDO = "Salvando…";
const TEXTO_SALVO = "Raciocínio salvo";

// Id da resposta cuja análise da IA já foi consultada nesta aba. Depois do F5 isso diz que o backend
// já tem a análise salva (pedir de novo devolve a salva, sem chamar o Gemini).
const CHAVE_CONSULTA_IA = "pensaAntes.consultaIaRespostaId";
const TEXTO_CONSULTANDO = "Consultando a IA...";
const TEXTO_CONSULTADO = "Análise consultada";
const TEXTO_AGUARDANDO_IA = "A IA está analisando o desafio. Isso pode levar alguns segundos.";

// Valores exatos que o backend aceita em decisaoFinal.
const DECISAO_MANTER = "ManterRespostaInicial";
const DECISAO_MODIFICAR = "ModificarComBaseNaIA";
const TEXTO_ENVIANDO_DECISAO = "Enviando a sua decisão…";
const TEXTO_FINALIZANDO = "Finalizando…";
const TEXTO_FINALIZADO = "Resposta finalizada";

// Id da resposta já finalizada nesta aba. Só melhora a interface depois do F5: o banco é a fonte de verdade.
const CHAVE_RESPOSTA_FINALIZADA = "pensaAntes.respostaFinalizadaId";
// Identifica a resposta cuja avaliação já foi concluída nesta aba.
// O resultado permanece salvo no backend.
const CHAVE_AVALIACAO_CONCLUIDA = "pensaAntes.avaliacaoConcluidaId";
// Identifica a resposta cuja autoavaliação já foi salva nesta aba.
// O PostgreSQL continua sendo a fonte de verdade.
const CHAVE_AUTOAVALIACAO_CONCLUIDA = "pensaAntes.autoavaliacaoConcluidaId";
const MENSAGEM_JA_FINALIZADA =
  "Esta resposta já foi finalizada e não pode ser finalizada de novo. A decisão enviada antes não é mostrada aqui, e o que você acabou de digitar não foi registrado.";
const MENSAGEM_FINALIZADA_RESTAURADA =
  "Esta resposta já foi finalizada. A decisão e a justificativa enviadas não são mostradas de novo nesta tela.";

// Chave: nome do nível sem acento e em minúsculas. Valor: número de pontos e texto exibido.
const NIVEIS = {
  facil: { numero: 1, rotulo: "Fácil" },
  medio: { numero: 2, rotulo: "Médio" },
  dificil: { numero: 3, rotulo: "Difícil" },
};

const saudacao = document.getElementById("saudacao");
const areaDesafio = document.getElementById("area-desafio");
const estadoCarregando = document.getElementById("estado-carregando");
const estadoNaoEncontrado = document.getElementById("estado-nao-encontrado");
const estadoErro = document.getElementById("estado-erro");
const mensagemErro = document.getElementById("mensagem-erro");
const botaoTentarNovamente = document.getElementById("botao-tentar-novamente");
const conteudoDesafio = document.getElementById("conteudo-desafio");
const titulo = document.getElementById("desafio-titulo");
const categoria = document.getElementById("desafio-categoria");
const nivel = document.getElementById("desafio-nivel");
const nivelTexto = document.getElementById("desafio-nivel-texto");
const situacao = document.getElementById("desafio-situacao");
const dados = document.getElementById("desafio-dados");
const objetivo = document.getElementById("desafio-objetivo");
const pergunta = document.getElementById("desafio-pergunta");
const formularioResposta = document.getElementById("form-resposta");
const campoAnalise = document.getElementById("analise-inicial");
const campoJustificativa = document.getElementById("justificativa-inicial");
const mensagemForm = document.getElementById("mensagem-form");
const botaoSalvar = formularioResposta?.querySelector('button[type="submit"]');
const secaoEscolhaIa = document.getElementById("secao-escolha-ia");
const botaoConsultarIa = document.getElementById("botao-consultar-ia");
const mensagemEscolha = document.getElementById("mensagem-escolha");
const secaoAnaliseIa = document.getElementById("secao-analise-ia");
const tituloAnaliseIa = document.getElementById("titulo-analise-ia");
const conteudoAnaliseIa = document.getElementById("conteudo-analise-ia");
const botaoContinuarSemIa = document.getElementById("botao-continuar-sem-ia");
const secaoDecisaoFinal = document.getElementById("secao-decisao-final");
const formularioDecisao = document.getElementById("form-decisao");
const tituloDecisao = document.getElementById("titulo-decisao");
const decisaoModificar = document.getElementById("decisao-modificar");
const notaDecisaoModificar = document.getElementById("nota-decisao-modificar");
const opcoesDecisao = document.getElementById("opcoes-decisao");
const decisaoManter = document.getElementById("decisao-manter");
const campoJustificativaFinal = document.getElementById("justificativa-final");
const mensagemDecisao = document.getElementById("mensagem-decisao");
const botaoFinalizar = document.getElementById("botao-finalizar-resposta");

// --- Elementos da avaliação automática ---

const secaoAvaliacao = document.getElementById("secao-avaliacao");
const tituloAvaliacao = document.getElementById("titulo-avaliacao");
const botaoAvaliar = document.getElementById("botao-avaliar-resposta");
const botaoPularAvaliacao = document.getElementById("botao-pular-avaliacao");
const botaoContinuarAposAvaliacao = document.getElementById("botao-continuar-apos-avaliacao");
const secaoAutoavaliacao = document.getElementById("secao-autoavaliacao");
const tituloAutoavaliacao = document.getElementById("titulo-autoavaliacao");
const formularioAutoavaliacao = document.getElementById("form-autoavaliacao");
const campoOQueFezBem = document.getElementById("autoavaliacao-pontos-fortes");
const campoOQuePodeMelhorar = document.getElementById("autoavaliacao-melhorias");
const botaoSalvarAutoavaliacao = document.getElementById("botao-salvar-autoavaliacao");
const botaoVerConclusao = document.getElementById("botao-ver-conclusao");
const mensagemAutoavaliacao = document.getElementById("mensagem-autoavaliacao");
const mensagemAvaliacao = document.getElementById("mensagem-avaliacao");
const resultadoAvaliacao = document.getElementById("resultado-avaliacao");

const avaliacaoTotal = document.getElementById("avaliacao-total");
const avaliacaoFeedback = document.getElementById("avaliacao-feedback");

const criteriosAvaliacao = {
  identificacao: {
    nota: document.getElementById("nota-identificacao"),
    barra: document.getElementById("barra-identificacao"),
  },
  usoDosDados: {
    nota: document.getElementById("nota-uso-dos-dados"),
    barra: document.getElementById("barra-uso-dos-dados"),
  },
  hipotese: {
    nota: document.getElementById("nota-hipotese"),
    barra: document.getElementById("barra-hipotese"),
  },
  justificativa: {
    nota: document.getElementById("nota-justificativa"),
    barra: document.getElementById("barra-justificativa"),
  },
  pensamentoCritico: {
    nota: document.getElementById("nota-pensamento-critico"),
    barra: document.getElementById("barra-pensamento-critico"),
  },
};

function iniciar(usuario) {
  // O id vem da URL: /desafio.html?id=1. Se não existir, vale null.
  // Quem valida é buscarDesafioPorId(): id ausente ou inválido vira "não encontrado", sem pedir nada à API.
  const id = new URLSearchParams(window.location.search).get("id");

  let buscando = false;
  let desafioAtual = null; // o desafio carregado da API (o formulário só aparece depois disso)
  let salvando = false;
  let salvo = false;
  let respostaIdAtual = null; // id da resposta salva (vem do POST ou da restauração), validado como inteiro positivo
  let consultando = false;
  let analiseMostrada = false;
  let seguirSemIa = false; // a pessoa clicou em "Continuar sem consultar a IA"
  let finalizando = false;
  let finalizado = false;
  // --- Estado da avaliação automática ---
  let avaliando = false;
  let avaliacaoMostrada = false;
  let salvandoAutoavaliacao = false;
  let autoavaliacaoSalva = false;

  const textoBotaoAvaliar = botaoAvaliar.textContent;
  const textoBotaoSalvar = botaoSalvar.textContent;
  const textoBotaoConsultar = botaoConsultarIa.textContent;
  const textoBotaoFinalizar = botaoFinalizar.textContent;

  // Desliga os avisos nativos do navegador para mostrarmos as nossas mensagens em português.
  formularioResposta.noValidate = true;

  // --- Tela -----------------------------------------------------------------

  // Mostra só o estado pedido: "carregando", "conteudo", "nao-encontrado" ou "erro".
  function mostrarEstado(estado) {
    estadoCarregando.hidden = estado !== "carregando";
    conteudoDesafio.hidden = estado !== "conteudo";
    estadoNaoEncontrado.hidden = estado !== "nao-encontrado";
    estadoErro.hidden = estado !== "erro";

    // aria-busy="true" avisa leitores de tela que a região ainda está sendo atualizada.
    areaDesafio.setAttribute("aria-busy", String(estado === "carregando"));
  }

  function primeiroNome(nome) {
    return nome.trim().split(/\s+/)[0];
  }

  // "Fácil", "facil" e "FACIL" viram "facil": tira acentos e maiúsculas.
  function chaveDoNivel(texto) {
    return String(texto ?? "")
      .normalize("NFD")
      .replace(/[\u0300-\u036f]/g, "")
      .trim()
      .toLowerCase();
  }

  function preencherNivel(dificuldade) {
    const conhecido = NIVEIS[chaveDoNivel(dificuldade)];
    const pontos = nivel.querySelector(".nivel__pontos");

    // O selo é fixo no HTML e serve para várias buscas: limpa o que sobrou da anterior.
    nivel.classList.remove("nivel--1", "nivel--2", "nivel--3");

    if (conhecido) {
      nivel.classList.add(`nivel--${conhecido.numero}`);
      nivelTexto.textContent = conhecido.rotulo;
      pontos.hidden = false;
    } else {
      // Nível desconhecido: mantém o texto original e esconde os pontos.
      nivelTexto.textContent = String(dificuldade ?? "");
      pontos.hidden = true;
    }
  }

  function preencherDesafio(desafio) {
    // textContent: o texto da API nunca é lido como HTML.
    titulo.textContent = String(desafio.titulo ?? "");
    categoria.textContent = String(desafio.categoria ?? "");
    situacao.textContent = String(desafio.situacaoProblema ?? "");
    dados.textContent = String(desafio.dadosDisponiveis ?? ""); // os "\n" viram linhas pelo CSS (pre-line)
    objetivo.textContent = String(desafio.objetivo ?? "");
    pergunta.textContent = String(desafio.perguntaPrincipal ?? "");

    preencherNivel(desafio.dificuldade);

    document.title = `${titulo.textContent} | Pensa Antes`;
  }

  // --- API ------------------------------------------------------------------

  function mensagemParaErro(erro) {
    if (erro instanceof ApiError) {
      return erro.message; // frase própria do api.js, segura para exibir
    }

    console.error("[desafio] erro inesperado:", erro);
    return "Algo deu errado. Tente de novo.";
  }

  async function carregar() {
    if (buscando) return; // já tem uma busca em andamento

    buscando = true;
    mostrarEstado("carregando");

    try {
      const desafio = await buscarDesafioPorId(id);

      if (!desafio || typeof desafio !== "object" || Array.isArray(desafio)) {
        // 200, mas sem o objeto esperado (por exemplo, uma página HTML no lugar do JSON).
        throw new ApiError(TIPO_ERRO.SERVIDOR, { detalheTecnico: "resposta sem os dados do desafio" });
      }

      preencherDesafio(desafio);
      desafioAtual = desafio;
      restaurarRespostaSalva();
      mostrarEstado("conteudo");

    } catch (erro) {
      if (erro instanceof ApiError && erro.tipo === TIPO_ERRO.NAO_ENCONTRADO) {
        // Vale para id ausente, id inválido e desafio inexistente (404).
        mostrarEstado("nao-encontrado");
      } else {
        mensagemErro.textContent = mensagemParaErro(erro);
        mostrarEstado("erro");
      }
    } finally {
      buscando = false;
    }
  }

  // --- Eventos --------------------------------------------------------------

  saudacao.textContent = `Olá, ${primeiroNome(usuario.nome)}!`;

  botaoTentarNovamente.addEventListener("click", carregar);

    // --- Formulário -----------------------------------------------------------

  // A mensagem do formulário serve para erro e para sucesso (a classe só muda a cor).
  function mostrarMensagemForm(texto, sucesso = false) {
    mensagemForm.textContent = texto; // textContent: nunca é lido como HTML
    mensagemForm.classList.toggle("campo__erro--sucesso", sucesso);
  }

  function marcarCampoInvalido(nome) {
    formularioResposta.elements[nome]?.setAttribute("aria-invalid", "true");
  }

  function limparErrosForm() {
    mostrarMensagemForm("");
    for (const campo of formularioResposta.querySelectorAll('[aria-invalid="true"]')) {
      campo.removeAttribute("aria-invalid");
    }
  }

  function definirSalvando(carregando) {
    botaoSalvar.disabled = carregando;
    botaoSalvar.textContent = carregando ? TEXTO_SALVANDO : textoBotaoSalvar;
    formularioResposta.setAttribute("aria-busy", String(carregando));
  }

  // Devolve a lista de problemas: [{ campo, mensagem }]. Lista vazia = tudo certo.
  function validarTextos(analise, justificativa) {
    const limite = LIMITE_TEXTO.toLocaleString("pt-BR");
    const problemas = [];

    if (analise === "") {
      problemas.push({ campo: "analiseInicial", mensagem: "Escreva a sua análise inicial." });
    } else if (analise.length > LIMITE_TEXTO) {
      problemas.push({ campo: "analiseInicial", mensagem: `A análise inicial pode ter até ${limite} caracteres.` });
    }

    if (justificativa === "") {
      problemas.push({ campo: "justificativaInicial", mensagem: "Escreva a sua justificativa inicial." });
    } else if (justificativa.length > LIMITE_TEXTO) {
      problemas.push({ campo: "justificativaInicial", mensagem: `A justificativa inicial pode ter até ${limite} caracteres.` });
    }

    return problemas;
  }

  // Único ponto que mostra ou esconde a escolha da IA: ela só aparece com o raciocínio salvo.
  function atualizarEscolhaIa() {
    secaoEscolhaIa.hidden = !salvo;
  }

  // Trava o formulário depois que a resposta foi salva (ou restaurada).
  function marcarComoSalvo() {
    salvo = true;
    botaoSalvar.disabled = true;
    botaoSalvar.textContent = TEXTO_SALVO;
    formularioResposta.setAttribute("aria-busy", "false");
    campoAnalise.readOnly = true;
    campoJustificativa.readOnly = true;
    atualizarEscolhaIa();
    atualizarBotoesDoFluxo(); // salvo mudou: o botão de consultar passa a poder ser usado
  }

  function guardarRespostaAtual(resposta) {
    try {
      // sessionStorage só guarda texto, por isso JSON.stringify.
      sessionStorage.setItem(CHAVE_RESPOSTA_SALVA, JSON.stringify(resposta));
      sessionStorage.setItem(CHAVE_RESPOSTA_ATUAL, String(resposta.respostaId));
      return true;
    } catch (erro) {
      console.warn("[desafio] não foi possível guardar a resposta no navegador:", erro);
      return false;
    }
  }

  // Devolve a resposta guardada nesta aba, ou null se não há nada que sirva.
  // Nunca lança erro e nunca escreve dados do usuário no console.
  function lerRespostaSalva() {
    let texto;

    try {
      texto = sessionStorage.getItem(CHAVE_RESPOSTA_SALVA);
    } catch {
      return null; // armazenamento indisponível
    }

    if (texto === null) return null; // nada guardado ainda

    let dados;

    try {
      dados = JSON.parse(texto);
    } catch {
      return null; // JSON quebrado
    }

    if (!dados || typeof dados !== "object") return null;

    const ehId = (valor) => Number.isInteger(valor) && valor > 0;
    const ehTexto = (valor) => typeof valor === "string" && valor.trim() !== "" && valor.length <= LIMITE_TEXTO;

    if (
      !ehId(dados.respostaId) ||
      !ehId(dados.usuarioId) ||
      !ehId(dados.desafioId) ||
      !ehTexto(dados.analiseInicial) ||
      !ehTexto(dados.justificativaInicial)
    ) {
      return null; // dados incompletos ou com tipos errados
    }

    // Devolve só os campos conhecidos.
    return {
      respostaId: dados.respostaId,
      usuarioId: dados.usuarioId,
      desafioId: dados.desafioId,
      analiseInicial: dados.analiseInicial,
      justificativaInicial: dados.justificativaInicial,
    };
  }

  // Restaura a tela (e só a tela) se a resposta guardada é DESTE usuário e DESTE desafio.
  // Não chama a API: sessionStorage não prova que a resposta ainda existe no servidor.
  // Se for de outro usuário ou desafio, não faz nada e também não apaga o que está guardado.
  function restaurarRespostaSalva() {
    if (salvo || salvando || !desafioAtual) return;

    const salva = lerRespostaSalva();

    if (!salva || salva.usuarioId !== usuario.id || salva.desafioId !== desafioAtual.id) return;

    campoAnalise.value = salva.analiseInicial;
    campoJustificativa.value = salva.justificativaInicial;
    respostaIdAtual = salva.respostaId; // já validado por lerRespostaSalva()
    marcarComoSalvo();
    mostrarMensagemForm("Seu raciocínio salvo foi restaurado.", true);

    // Resposta já finalizada nesta aba: mostra a decisão travada, sem inventar os dados enviados.
    if (finalizacaoJaRegistrada(respostaIdAtual)) {
      concluirFinalizacao(MENSAGEM_FINALIZADA_RESTAURADA, false);
    }

    // Só recupera a análise se esta aba já a consultou: o backend devolve a salva, sem chamar o Gemini.
    // Sem essa confirmação, nada é consultado sozinho.
    if (consultaJaConcluida(respostaIdAtual)) {
      consultarIa({ usuarioIniciou: false });
    }

    // Se a avaliação já foi concluída nesta aba,
    // recupera o resultado salvo no backend.
    if (
      finalizado &&
      avaliacaoJaConcluida(respostaIdAtual)
    ) {
      solicitarAvaliacao({ restaurando: true });
    }
  }
  
  // Recupera o estado de conclusão da autoavaliação,
  // sem reenviar os textos ao backend.
  restaurarAutoavaliacaoConcluida();

  function mensagemParaErroDeEnvio(erro) {
    if (erro instanceof ApiError && erro.tipo === TIPO_ERRO.NAO_ENCONTRADO) {
      // A API responde 404 tanto para usuário quanto para desafio inexistente.
      return "Não encontramos o seu cadastro ou este desafio. Volte ao início e identifique-se de novo.";
    }

    return mensagemParaErro(erro);
  }

  // Volta o formulário ao normal para permitir uma nova tentativa.
  function terminarComErro(mensagem, camposInvalidos = []) {
    salvando = false;
    definirSalvando(false);
    mostrarMensagemForm(mensagem);
    camposInvalidos.forEach(marcarCampoInvalido);
  }

  formularioResposta.addEventListener("submit", async (evento) => {
    evento.preventDefault(); // impede o navegador de recarregar a página com os textos na URL

    if (salvando || salvo || !desafioAtual) return; // evita envio duplicado

    limparErrosForm();

    const analise = campoAnalise.value.trim();
    const justificativa = campoJustificativa.value.trim();
    const problemas = validarTextos(analise, justificativa);

    if (problemas.length > 0) {
      problemas.forEach((problema) => marcarCampoInvalido(problema.campo));
      mostrarMensagemForm(problemas.map((problema) => problema.mensagem).join(" "));
      formularioResposta.elements[problemas[0].campo].focus();
      return;
    }

    salvando = true;
    definirSalvando(true);

    try {
      const resposta = await criarRespostaInicial({
        usuarioId: usuario.id,
        desafioId: desafioAtual.id,
        analiseInicial: analise,
        justificativaInicial: justificativa,
      });

      if (!Number.isInteger(resposta?.id) || resposta.id <= 0) {
        console.error("[desafio] resposta sem id válido:", resposta);
        terminarComErro("Não conseguimos confirmar que o seu raciocínio foi salvo. Tente de novo.");
        return;
      }

      // Salvo: o botão continua desabilitado e os textos ficam só para leitura,
      // para ninguém criar outra resposta sem querer nem editar algo que já foi enviado.
      salvando = false;
      respostaIdAtual = resposta.id; // fica em memória: a consulta à IA não depende do sessionStorage
      marcarComoSalvo();

      const guardou = guardarRespostaAtual({
        respostaId: resposta.id,
        usuarioId: usuario.id,
        desafioId: desafioAtual.id,
        analiseInicial: analise,
        justificativaInicial: justificativa,
      });

      if (guardou) {
        mostrarMensagemForm("Seu raciocínio foi salvo.", true);
      } else {
        mostrarMensagemForm(
          "Seu raciocínio foi salvo, mas o navegador não permitiu guardar o andamento. Libere o armazenamento do site nas configurações do navegador para continuar."
        );
      }
    } catch (erro) {
      const campos = erro instanceof ApiError ? erro.campos : [];
      terminarComErro(mensagemParaErroDeEnvio(erro), campos);
    }
  });

  // --- Consulta à IA --------------------------------------------------------

  // Mensagem da escolha: serve para "aguardando" (neutra) e para erro. A classe só muda a cor.
  function mostrarMensagemEscolha(texto, neutra = false) {
    mensagemEscolha.textContent = texto; // textContent: nunca é lido como HTML
    mensagemEscolha.classList.toggle("campo__erro--sucesso", neutra);
  }

  function lembrarConsultaConcluida(id) {
    try {
      sessionStorage.setItem(CHAVE_CONSULTA_IA, String(id));
    } catch {
      // Sem armazenamento: a consulta continua valendo enquanto esta página estiver aberta.
    }
  }

  function consultaJaConcluida(id) {
    try {
      return sessionStorage.getItem(CHAVE_CONSULTA_IA) === String(id);
    } catch {
      return false;
    }
  }

  // Devolve o texto da análise se a resposta tem o formato esperado e é da resposta certa; senão, null.
  function extrairAnalise(resposta) {
    if (!resposta || typeof resposta !== "object" || Array.isArray(resposta)) return null;
    if (resposta.respostaId !== respostaIdAtual) return null;
    if (resposta.consultouIA !== true) return null;
    if (typeof resposta.analiseIA !== "string" || resposta.analiseIA.trim() === "") return null;

    return resposta.analiseIA.trim();
  }

  function mensagemParaErroDeConsulta(erro) {
    if (erro instanceof ApiError) {
      if (erro.tipo === TIPO_ERRO.NAO_ENCONTRADO) {
        return "Não encontramos o seu raciocínio salvo no servidor. Volte aos desafios e comece de novo.";
      }

      if (erro.tipo === TIPO_ERRO.SERVIDOR) {
        return "A IA não conseguiu responder agora. O seu raciocínio continua salvo. Tente de novo em instantes.";
      }

      if (erro.tipo === TIPO_ERRO.DADOS_INVALIDOS) {
        return "Não foi possível consultar a IA com este raciocínio. Recarregue a página e tente de novo.";
      }
    }

    return mensagemParaErro(erro); // rede e demais casos
  }

  // usuarioIniciou: true no clique; false quando a análise é recuperada depois do F5 (sem mexer no foco).
  async function consultarIa({ usuarioIniciou }) {
    if (consultando || analiseMostrada || !salvo || respostaIdAtual === null) return; // evita consultas simultâneas
    if (usuarioIniciou && (finalizando || finalizado)) return; // depois de finalizar, só a restauração recupera a análise

    consultando = true;
    atualizarBotaoContinuar();
    botaoConsultarIa.textContent = TEXTO_CONSULTANDO;
    secaoEscolhaIa.setAttribute("aria-busy", "true");
    mostrarMensagemEscolha(TEXTO_AGUARDANDO_IA, true);

    try {
      const resposta = await consultarAnaliseIa(respostaIdAtual);
      const analise = extrairAnalise(resposta);

      if (analise === null) {
        // Só os nomes das propriedades: o conteúdo pode ter o texto da IA e do usuário.
        console.error(
          "[desafio] resposta da consulta à IA fora do formato esperado. Propriedades:",
          resposta && typeof resposta === "object" ? Object.keys(resposta) : typeof resposta
        );
        throw new ApiError(TIPO_ERRO.SERVIDOR, { detalheTecnico: "resposta da consulta sem os dados esperados" });
      }

      conteudoAnaliseIa.textContent = analise; // textContent: quebras de linha pelo CSS (pre-line), nunca HTML
      secaoAnaliseIa.hidden = false;
      analiseMostrada = true;
      atualizarDecisaoFinal(); // com a análise, a opção "Modificar com base na IA" passa a valer
      botaoConsultarIa.textContent = TEXTO_CONSULTADO; // continua desabilitado
      mostrarMensagemEscolha("");
      lembrarConsultaConcluida(respostaIdAtual);

      if (usuarioIniciou) tituloAnaliseIa.focus();
      } catch (erro) {
      consultando = false;
      atualizarBotoesDoFluxo(); // libera o botão (se as regras permitirem) antes de devolver o foco
      botaoConsultarIa.textContent = textoBotaoConsultar;
      botaoConsultarIa.textContent = textoBotaoConsultar;
      mostrarMensagemEscolha(mensagemParaErroDeConsulta(erro));

      if (usuarioIniciou) botaoConsultarIa.focus(); // o foco se perde quando o botão é desabilitado
    } finally {
      consultando = false;
      atualizarBotoesDoFluxo();
    }
  }

  botaoConsultarIa.addEventListener("click", () => consultarIa({ usuarioIniciou: true }));

  
  // --- Decisão final (só a interface; o envio vem em outra etapa) ------------

  // Único ponto que mostra a decisão e liga/desliga a opção da IA. Só mexe em hidden e disabled:
  // nunca limpa a justificativa nem troca a opção que a pessoa já escolheu.
  function atualizarDecisaoFinal() {
    const podeModificar = analiseMostrada;

    decisaoModificar.disabled = !podeModificar;
    notaDecisaoModificar.hidden = podeModificar;

    // A decisão só existe com o raciocínio salvo e depois de um dos dois caminhos.
    secaoDecisaoFinal.hidden = !(salvo && (seguirSemIa || analiseMostrada || finalizado));
  }

  // "Continuar sem IA" não vale enquanto há consulta em andamento, nem depois que já houve um caminho.
  function atualizarBotaoContinuar() {
    botaoContinuarSemIa.disabled = consultando || seguirSemIa || analiseMostrada || finalizado;
  }

  botaoContinuarSemIa.addEventListener("click", () => {
    if (!salvo || consultando || seguirSemIa || analiseMostrada) return;

    seguirSemIa = true;
    atualizarDecisaoFinal();
    atualizarBotaoContinuar();
    tituloDecisao.focus(); // o botão clicado acabou de ser desabilitado: leva o foco para o que apareceu
  });

  // --- Avaliação automática -------------------------------------------------

  function atualizarSecaoAvaliacao() {
    // A avaliação só fica disponível depois da finalização.
    secaoAvaliacao.hidden = !finalizado;
  }
  

function mostrarMensagemAvaliacao(texto, sucesso = false) {
  mensagemAvaliacao.textContent = texto;
  mensagemAvaliacao.classList.toggle(
    "campo__erro--sucesso",
    sucesso
  );
}

function definirAvaliando(ativo) {
  avaliando = ativo;

  botaoAvaliar.disabled = ativo || avaliacaoMostrada;
  botaoAvaliar.textContent = ativo
    ? "Avaliando seu raciocínio..."
    : avaliacaoMostrada
      ? "Avaliação concluída"
      : textoBotaoAvaliar;

  secaoAvaliacao.setAttribute("aria-busy", String(ativo));
}

function validarDadosAvaliacao(dados) {
  if (!dados || typeof dados !== "object" || Array.isArray(dados)) {
    return false;
  }

  if (dados.respostaId !== respostaIdAtual) {
    return false;
  }

  const nomes = Object.keys(criteriosAvaliacao);

  const notasValidas = nomes.every(
    (nome) =>
      Number.isInteger(dados[nome]) &&
      dados[nome] >= 0 &&
      dados[nome] <= 3
  );

  if (!notasValidas) return false;

  const soma = nomes.reduce((total, nome) => total + dados[nome], 0);

  return (
    Number.isInteger(dados.total) &&
    dados.total === soma &&
    typeof dados.feedback === "string" &&
    dados.feedback.trim() !== ""
  );
}

function preencherAvaliacao(dados) {
  avaliacaoTotal.textContent = String(dados.total);
  avaliacaoFeedback.textContent = dados.feedback;

  for (const [nome, elementos] of Object.entries(criteriosAvaliacao)) {
    const nota = dados[nome];

    elementos.nota.textContent = `${nota} / 3`;
    elementos.barra.style.setProperty(
      "--progresso",
      `${(nota / 3) * 100}%`
    );
    elementos.barra.setAttribute("aria-valuenow", String(nota));
  }

  resultadoAvaliacao.hidden = false;
  avaliacaoMostrada = true;
  definirAvaliando(false);
}

async function solicitarAvaliacao({ restaurando = false } = {}) {
  if (
    avaliando ||
    avaliacaoMostrada ||
    !finalizado ||
    !Number.isInteger(respostaIdAtual) ||
    respostaIdAtual <= 0
  ) {
    return;
  }

  definirAvaliando(true);

  mostrarMensagemAvaliacao(
    "A IA está avaliando seu raciocínio. Aguarde alguns segundos.",
    true
  );

  try {
    const dados = await avaliarResposta(respostaIdAtual);

    if (!validarDadosAvaliacao(dados)) {
      throw new ApiError(TIPO_ERRO.SERVIDOR, {
        detalheTecnico: "avaliação retornou dados inválidos",
      });
    }

    preencherAvaliacao(dados);
    // Registra que esta resposta já possui uma avaliação válida.
    lembrarAvaliacaoConcluida(respostaIdAtual);
    mostrarMensagemAvaliacao(
      "Sua avaliação foi concluída!",
      true
    );

    if (!restaurando) tituloAvaliacao.focus();

  } catch (erro) {
    if (erro instanceof ApiError && erro.tipo === TIPO_ERRO.NAO_ENCONTRADO) {
      mostrarMensagemAvaliacao(
        "Não encontramos sua resposta no servidor."
      );
    } else if (erro instanceof ApiError && erro.tipo === TIPO_ERRO.CONFLITO) {
      mostrarMensagemAvaliacao(
        "Esta resposta ainda não pode ser avaliada. Verifique se ela foi finalizada."
      );
    } else if (erro instanceof ApiError && erro.tipo === TIPO_ERRO.SERVIDOR) {
      mostrarMensagemAvaliacao(
        "A IA não conseguiu concluir a avaliação agora. Sua resposta continua salva. Tente novamente."
      );
    } else {
      mostrarMensagemAvaliacao(mensagemParaErro(erro));
    }

    if (!restaurando) botaoAvaliar.focus();

  } finally {
    definirAvaliando(false);
  }
}

function lembrarAvaliacaoConcluida(id) {
  try {
    sessionStorage.setItem(CHAVE_AVALIACAO_CONCLUIDA, String(id));
  } catch {
    // Sem armazenamento, a avaliação continua disponível nesta página.
  }
}

function avaliacaoJaConcluida(id) {
  try {
    return sessionStorage.getItem(CHAVE_AVALIACAO_CONCLUIDA) === String(id);
  } catch {
    return false;
  }
}

function lembrarAutoavaliacaoConcluida(id) {
  try {
    sessionStorage.setItem(CHAVE_AUTOAVALIACAO_CONCLUIDA, String(id));
  } catch {
    // Se o armazenamento estiver indisponível,
    // o salvamento no backend continua válido.
  }
}

function autoavaliacaoJaConcluida(id) {
  try {
    return sessionStorage.getItem(CHAVE_AUTOAVALIACAO_CONCLUIDA) === String(id);
  } catch {
    return false;
  }
}

function restaurarAutoavaliacaoConcluida() {
  if (
    !finalizado ||
    !Number.isInteger(respostaIdAtual) ||
    !autoavaliacaoJaConcluida(respostaIdAtual)
  ) {
    return;
  }

  autoavaliacaoSalva = true;

  secaoAutoavaliacao.hidden = false;
  botaoPularAvaliacao.hidden = true;

  campoOQueFezBem.readOnly = true;
  campoOQuePodeMelhorar.readOnly = true;

  botaoSalvarAutoavaliacao.disabled = true;
  botaoSalvarAutoavaliacao.textContent = "Reflexão concluída";

  mensagemAutoavaliacao.textContent =
    "Sua reflexão já foi registrada. Os textos enviados anteriormente não são exibidos nesta tela.";

  mensagemAutoavaliacao.classList.add("campo__erro--sucesso");
}


async function recuperarAutoavaliacaoSalva() {
  if (
    !finalizado ||
    !Number.isInteger(respostaIdAtual) ||
    respostaIdAtual <= 0
  ) {
    return false;
  }

  // Se já recuperamos a autoavaliação nesta página,
  // não precisamos consultar novamente.
  if (autoavaliacaoSalva) {
    return true;
  }

  const idConsultado = respostaIdAtual;

  try {
    const resultado = await buscarAutoavaliacao(idConsultado);

    if (
      !resultado ||
      resultado.respostaId !== idConsultado ||
      !Number.isInteger(resultado.id) ||
      resultado.id <= 0 ||
      typeof resultado.oQueFezBem !== "string" ||
      typeof resultado.oQuePodeMelhorar !== "string"
    ) {
      throw new ApiError(TIPO_ERRO.SERVIDOR, {
        detalheTecnico: "autoavaliação retornou dados inválidos"
      });
    }

    if (respostaIdAtual !== idConsultado) {
      return false;
    }

    autoavaliacaoSalva = true;
    lembrarAutoavaliacaoConcluida(idConsultado);
    liberarConclusao();

    campoOQueFezBem.value = resultado.oQueFezBem;
    campoOQuePodeMelhorar.value = resultado.oQuePodeMelhorar;

    campoOQueFezBem.readOnly = true;
    campoOQuePodeMelhorar.readOnly = true;

    botaoSalvarAutoavaliacao.disabled = true;
    botaoSalvarAutoavaliacao.textContent = "Reflexão concluída";

    mensagemAutoavaliacao.textContent =
      "Sua reflexão já foi registrada e recuperada com sucesso!";

    mensagemAutoavaliacao.classList.add("campo__erro--sucesso");

    return true;

  } catch (erro) {
    if (
      erro instanceof ApiError &&
      erro.tipo === TIPO_ERRO.NAO_ENCONTRADO
    ) {
      // Consulta concluída: ainda não existe autoavaliação.
      return true;
    }

    console.error("[desafio] erro ao recuperar autoavaliação:", erro);

    mensagemAutoavaliacao.textContent =
      "Não foi possível verificar sua reflexão anterior. Tente atualizar a página.";

    mensagemAutoavaliacao.classList.remove("campo__erro--sucesso");

    return false;
  }
}

async function mostrarAutoavaliacao() {
  if (!finalizado) return;

  secaoAutoavaliacao.hidden = false;
  botaoPularAvaliacao.hidden = true;

  // Impede um novo envio enquanto verificamos o PostgreSQL.
  botaoSalvarAutoavaliacao.disabled = true;
  formularioAutoavaliacao.setAttribute("aria-busy", "true");

  mensagemAutoavaliacao.textContent =
    "Verificando se sua reflexão já foi registrada...";

  let verificacaoConcluida = false;

  try {
    verificacaoConcluida = await recuperarAutoavaliacaoSalva();
  } finally {
      formularioAutoavaliacao.setAttribute("aria-busy", "false");

    // Só libera o envio se ainda não existe uma reflexão salva.
    if (verificacaoConcluida && !autoavaliacaoSalva) {
      botaoSalvarAutoavaliacao.disabled = false;
      botaoSalvarAutoavaliacao.textContent = "Concluir minha reflexão";
    }
  }

  tituloAutoavaliacao.focus();
}

function liberarConclusao() {
  if (
    !autoavaliacaoSalva ||
    !Number.isInteger(respostaIdAtual) ||
    respostaIdAtual <= 0
  ) {
    return;
  }

  botaoVerConclusao.href =
    `conclusao.html?respostaId=${encodeURIComponent(respostaIdAtual)}`;

  botaoVerConclusao.hidden = false;
}

botaoPularAvaliacao.addEventListener("click", mostrarAutoavaliacao);

botaoContinuarAposAvaliacao.addEventListener("click", mostrarAutoavaliacao);


// --- Salvamento da autoavaliação -----------------------------------------

  formularioAutoavaliacao.noValidate = true;

  formularioAutoavaliacao.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    if (
      salvandoAutoavaliacao ||
      autoavaliacaoSalva ||
      !finalizado ||
      !Number.isInteger(respostaIdAtual) ||
      respostaIdAtual <= 0
    ) {
      return;
    }

    const oQueFezBem = campoOQueFezBem.value.trim();
    const oQuePodeMelhorar = campoOQuePodeMelhorar.value.trim();

    mensagemAutoavaliacao.textContent = "";
    mensagemAutoavaliacao.classList.remove("campo__erro--sucesso");

    campoOQueFezBem.removeAttribute("aria-invalid");
    campoOQuePodeMelhorar.removeAttribute("aria-invalid");

    if (!oQueFezBem || oQueFezBem.length > 2000) {
      mensagemAutoavaliacao.textContent =
        "Escreva o que você fez bem, usando até 2.000 caracteres.";
      campoOQueFezBem.setAttribute("aria-invalid", "true");
      campoOQueFezBem.focus();
      return;
    }

    if (!oQuePodeMelhorar || oQuePodeMelhorar.length > 2000) {
      mensagemAutoavaliacao.textContent =
        "Escreva o que você poderia melhorar, usando até 2.000 caracteres.";
      campoOQuePodeMelhorar.setAttribute("aria-invalid", "true");
      campoOQuePodeMelhorar.focus();
      return;
    }

    salvandoAutoavaliacao = true;
    botaoSalvarAutoavaliacao.disabled = true;
    botaoSalvarAutoavaliacao.textContent = "Salvando sua reflexão...";
    formularioAutoavaliacao.setAttribute("aria-busy", "true");

    try {
      const resultado = await criarAutoavaliacao(respostaIdAtual, {
        oQueFezBem,
        oQuePodeMelhorar
      });

      if (
        !resultado ||
        resultado.respostaId !== respostaIdAtual ||
        !Number.isInteger(resultado.id) ||
        resultado.id <= 0
      ) {
        throw new ApiError(TIPO_ERRO.SERVIDOR, {
          detalheTecnico: "autoavaliação retornou dados inválidos"
        });
      }

      autoavaliacaoSalva = true;
      lembrarAutoavaliacaoConcluida(respostaIdAtual);
      liberarConclusao();

      campoOQueFezBem.value = resultado.oQueFezBem;
      campoOQuePodeMelhorar.value = resultado.oQuePodeMelhorar;

      campoOQueFezBem.readOnly = true;
      campoOQuePodeMelhorar.readOnly = true;

      botaoSalvarAutoavaliacao.textContent = "Reflexão concluída";

      mensagemAutoavaliacao.textContent =
        "Sua reflexão foi salva com sucesso!";
      mensagemAutoavaliacao.classList.add("campo__erro--sucesso");

      tituloAutoavaliacao.focus();

    } catch (erro) {
      if (erro instanceof ApiError && erro.tipo === TIPO_ERRO.CONFLITO) {
      autoavaliacaoSalva = true;
      lembrarAutoavaliacaoConcluida(respostaIdAtual);
      liberarConclusao();

      campoOQueFezBem.readOnly = true;
      campoOQuePodeMelhorar.readOnly = true;

      botaoSalvarAutoavaliacao.disabled = true;
      botaoSalvarAutoavaliacao.textContent = "Reflexão já registrada";

      mensagemAutoavaliacao.textContent =
        "Esta resposta já possui uma autoavaliação registrada. Os textos digitados agora não foram salvos novamente.";

      mensagemAutoavaliacao.classList.add("campo__erro--sucesso");
    } else if (erro instanceof ApiError && erro.tipo === TIPO_ERRO.NAO_ENCONTRADO) {
        mensagemAutoavaliacao.textContent =
          "Não encontramos sua resposta no servidor.";
      } else {
        mensagemAutoavaliacao.textContent = mensagemParaErro(erro);
      }

      if (!autoavaliacaoSalva) {
        botaoSalvarAutoavaliacao.disabled = false;
        botaoSalvarAutoavaliacao.textContent = "Concluir minha reflexão";
      }

    } finally {
      salvandoAutoavaliacao = false;
      formularioAutoavaliacao.setAttribute("aria-busy", "false");
    }
  });

// Conecta o botão à função de avaliação.
botaoAvaliar.addEventListener("click", () => {
  solicitarAvaliacao();
});

// --- Finalizar a resposta -------------------------------------------------

  // Desliga os avisos nativos do navegador para mostrarmos as nossas mensagens em português.
  formularioDecisao.noValidate = true;

  // A mensagem da decisão serve para erro e para aviso/sucesso (a classe só muda a cor).
  function mostrarMensagemDecisao(texto, neutra = false) {
    mensagemDecisao.textContent = texto; // textContent: nunca é lido como HTML
    mensagemDecisao.classList.toggle("campo__erro--sucesso", neutra);
  }

  function marcarDecisaoInvalida() {
    for (const radio of formularioDecisao.querySelectorAll('input[name="decisaoFinal"]')) {
      radio.setAttribute("aria-invalid", "true");
    }
  }

  function limparErrosDecisao() {
    mostrarMensagemDecisao("");
    for (const campo of formularioDecisao.querySelectorAll('[aria-invalid="true"]')) {
      campo.removeAttribute("aria-invalid");
    }
  }

  // Regras de habilitação dos três botões do fluxo, todas num lugar só. Chame atualizarBotoesDoFluxo()
  // sempre que salvo, respostaIdAtual, consultando, analiseMostrada, seguirSemIa, finalizando ou finalizado mudarem.
  function atualizarBotaoConsultar() {
    botaoConsultarIa.disabled =
      !salvo || respostaIdAtual === null || consultando || analiseMostrada || finalizando || finalizado;
  }

  function atualizarBotaoFinalizar() {
    botaoFinalizar.disabled = !salvo || respostaIdAtual === null || consultando || finalizando || finalizado;
  }

  function atualizarBotoesDoFluxo() {
    atualizarBotaoContinuar();
    atualizarBotaoConsultar();
    atualizarBotaoFinalizar();
  }

  // Marcador local de que esta resposta já foi finalizada. É só conforto de interface (o banco é a fonte de verdade).
  function lembrarFinalizacao(id) {
    try {
      sessionStorage.setItem(CHAVE_RESPOSTA_FINALIZADA, String(id));
    } catch {
      // Sem armazenamento: vale só enquanto esta página estiver aberta.
    }
  }

  function finalizacaoJaRegistrada(id) {
    try {
      return sessionStorage.getItem(CHAVE_RESPOSTA_FINALIZADA) === String(id);
    } catch {
      return false;
    }
  }

  function definirFinalizando(ativo) {
    finalizando = ativo;
    atualizarBotoesDoFluxo();
    botaoFinalizar.textContent = ativo ? TEXTO_FINALIZANDO : textoBotaoFinalizar;
    formularioDecisao.setAttribute("aria-busy", String(ativo));
    opcoesDecisao.disabled = ativo; // desabilita os rádios sem apagar o "disabled" próprio de cada um
    campoJustificativaFinal.readOnly = ativo;
  }

  // Trava tudo: a resposta já foi finalizada (agora, antes de um F5, ou o backend informou que já estava).
  // Não preenche nem escolhe nada: os dados da decisão já enviada não são conhecidos aqui.
  function concluirFinalizacao(mensagem, moverFoco = true) {
    finalizando = false;
    finalizado = true;
    botaoFinalizar.textContent = TEXTO_FINALIZADO;
    formularioDecisao.setAttribute("aria-busy", "false");
    opcoesDecisao.disabled = true;
    campoJustificativaFinal.readOnly = true;
    mostrarMensagemDecisao(mensagem, true);
    atualizarDecisaoFinal(); // finalizada: a seção da decisão aparece, mesmo sem consulta nem "continuar sem IA"
    atualizarBotoesDoFluxo();
    atualizarSecaoAvaliacao();

    if (moverFoco) tituloDecisao.focus(); // o botão clicado acabou de ser desabilitado (no F5 não mexe no foco)
  }

  // Devolve a lista de problemas: [{ campo, mensagem }]. Lista vazia = tudo certo.
  function validarDecisaoFinal(decisao, justificativa) {
    const limite = LIMITE_TEXTO.toLocaleString("pt-BR");
    const problemas = [];

    if (decisao !== DECISAO_MANTER && decisao !== DECISAO_MODIFICAR) {
      problemas.push({ campo: "decisaoFinal", mensagem: "Escolha uma decisão." });
    } else if (decisao === DECISAO_MODIFICAR && !analiseMostrada) {
      problemas.push({
        campo: "decisaoFinal",
        mensagem: "Só é possível modificar a resposta com base na IA depois de consultar a análise dela.",
      });
    }

    if (justificativa === "") {
      problemas.push({ campo: "justificativaFinal", mensagem: "Escreva a sua justificativa final." });
    } else if (justificativa.length > LIMITE_TEXTO) {
      problemas.push({ campo: "justificativaFinal", mensagem: `A justificativa final pode ter até ${limite} caracteres.` });
    }

    return problemas;
  }

  function mensagemParaErroDeFinalizacao(erro) {
    if (erro instanceof ApiError) {
      if (erro.tipo === TIPO_ERRO.NAO_ENCONTRADO) {
        return "Não encontramos o seu raciocínio salvo no servidor. Volte aos desafios e comece de novo.";
      }

      if (erro.tipo === TIPO_ERRO.DADOS_INVALIDOS) {
        return "Confira a sua decisão e a sua justificativa e tente de novo.";
      }

      if (erro.tipo === TIPO_ERRO.SERVIDOR) {
        return "Não foi possível finalizar agora. O seu raciocínio continua salvo. Tente de novo em instantes.";
      }
    }

    return mensagemParaErro(erro); // rede e demais casos
  }

  formularioDecisao.addEventListener("submit", async (evento) => {
    evento.preventDefault(); // impede o navegador de recarregar a página com a justificativa na URL

    if (finalizando || finalizado || !salvo || respostaIdAtual === null) return; // evita envio duplicado

    limparErrosDecisao();

    if (consultando) {
      mostrarMensagemDecisao("Aguarde a consulta à IA terminar para finalizar.");
      return;
    }

    const marcada = formularioDecisao.querySelector('input[name="decisaoFinal"]:checked');
    const decisao = marcada ? marcada.value : "";
    const justificativa = campoJustificativaFinal.value.trim();
    const problemas = validarDecisaoFinal(decisao, justificativa);

    if (problemas.length > 0) {
      for (const problema of problemas) {
        if (problema.campo === "decisaoFinal") {
          marcarDecisaoInvalida();
        } else {
          campoJustificativaFinal.setAttribute("aria-invalid", "true");
        }
      }

      mostrarMensagemDecisao(problemas.map((problema) => problema.mensagem).join(" "));
      (problemas[0].campo === "decisaoFinal" ? decisaoManter : campoJustificativaFinal).focus();
      return;
    }

    definirFinalizando(true);
    mostrarMensagemDecisao(TEXTO_ENVIANDO_DECISAO, true);

    try {
      const resposta = await finalizarResposta(respostaIdAtual, decisao, justificativa);

      if (!resposta || typeof resposta !== "object" || resposta.id !== respostaIdAtual) {
        // Nada do conteúdo vai para o console: só o aviso.
        console.error("[desafio] resposta da finalização fora do formato esperado.");
        throw new ApiError(TIPO_ERRO.SERVIDOR, { detalheTecnico: "resposta da finalização sem os dados esperados" });
      }

      lembrarFinalizacao(respostaIdAtual);
      concluirFinalizacao("Resposta finalizada! A sua decisão e a sua justificativa foram registradas.");
    } catch (erro) {
      if (erro instanceof ApiError && erro.tipo === TIPO_ERRO.CONFLITO) {
        // 409: o backend diz que esta resposta já foi finalizada. Não adianta tentar de novo.
        lembrarFinalizacao(respostaIdAtual);
        concluirFinalizacao(MENSAGEM_JA_FINALIZADA);
        return;
      }

      definirFinalizando(false);
      mostrarMensagemDecisao(mensagemParaErroDeFinalizacao(erro));

      if (erro instanceof ApiError && erro.tipo === TIPO_ERRO.DADOS_INVALIDOS) {
        if (erro.campos.includes("decisaoFinal")) marcarDecisaoInvalida();
        if (erro.campos.includes("justificativaFinal")) campoJustificativaFinal.setAttribute("aria-invalid", "true");
      }

      botaoFinalizar.focus(); // o foco se perde quando o botão é desabilitado
    }
  });

  atualizarDecisaoFinal(); // estado inicial: decisão oculta e opção da IA indisponível
  atualizarBotoesDoFluxo();
  atualizarSecaoAvaliacao();

  // Ao voltar pelo botão "voltar", o navegador pode reaproveitar a página como estava.
  // Se a identificação não existe mais, a pessoa volta para a Home.
  window.addEventListener("pageshow", (evento) => {
    if (evento.persisted && !recuperarUsuario()) {
      window.location.replace(PAGINA_HOME);
    }
  });

  carregar();
}

const elementosPresentes = [
  saudacao,
  areaDesafio,
  estadoCarregando,
  estadoNaoEncontrado,
  estadoErro,
  mensagemErro,
  botaoTentarNovamente,
  conteudoDesafio,
  titulo,
  categoria,
  nivel,
  nivelTexto,
  situacao,
  dados,
  objetivo,
  pergunta,
  formularioResposta,
  campoAnalise,
  campoJustificativa,
  mensagemForm,
  botaoSalvar,
  secaoEscolhaIa,
  botaoConsultarIa,
  mensagemEscolha,
  secaoAnaliseIa,
  tituloAnaliseIa,
  conteudoAnaliseIa,
  botaoContinuarSemIa,
  secaoDecisaoFinal,
  formularioDecisao,
  tituloDecisao,
  decisaoModificar,
  notaDecisaoModificar,
  opcoesDecisao,
  decisaoManter,
  campoJustificativaFinal,
  mensagemDecisao,
  botaoFinalizar,
  botaoContinuarAposAvaliacao,
  secaoAvaliacao,
  tituloAvaliacao,
  botaoAvaliar,
  mensagemAvaliacao,
  resultadoAvaliacao,
  avaliacaoTotal,
  avaliacaoFeedback,
  ...Object.values(criteriosAvaliacao).flatMap(
    (criterio) => [criterio.nota, criterio.barra]
  ),
  botaoPularAvaliacao,
  secaoAutoavaliacao,
  tituloAutoavaliacao,
  formularioAutoavaliacao,
  campoOQueFezBem,
  campoOQuePodeMelhorar,
  botaoSalvarAutoavaliacao,
  mensagemAutoavaliacao,
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