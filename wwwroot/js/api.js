/**
 * api.js: comunicação HTTP entre o frontend e a API do Pensa Antes.
 *
 * - Usa URLs relativas, porque frontend e API são servidos pela mesma aplicação.
 * - Só conversa com a API e traduz os erros. Não guarda nada no navegador
 *   e não mexe na tela: quem chama decide o que mostrar.
 * - Todo erro lançado é um ApiError. A propriedade `message` é sempre uma
 *   frase própria em português, segura para exibir. O texto técnico que a API
 *   devolve fica em `detalheTecnico` e serve só para depuração.
 */

const URL_BASE = "/api";

/** Tipos de erro que o resto do frontend pode checar em `erro.tipo`. */
export const TIPO_ERRO = Object.freeze({
  REDE: "rede",                          // não houve resposta (servidor fora do ar, sem conexão)
  DADOS_INVALIDOS: "dados-invalidos",    // HTTP 400
  NAO_ENCONTRADO: "nao-encontrado",      // HTTP 404
  EMAIL_JA_CADASTRADO: "email-ja-cadastrado", // HTTP 409 ao cadastrar usuário
  CONFLITO: "conflito",                  // outro HTTP 409
  SERVIDOR: "servidor",                  // HTTP 5xx (inclui 502, falha da IA)
  DESCONHECIDO: "desconhecido",          // qualquer outro status
});

const MENSAGENS = Object.freeze({
  [TIPO_ERRO.REDE]: "Não foi possível falar com o servidor. Confira sua conexão e tente de novo.",
  [TIPO_ERRO.DADOS_INVALIDOS]: "Confira os dados informados e tente de novo.",
  [TIPO_ERRO.NAO_ENCONTRADO]: "Não encontramos o que você procurou.",
  [TIPO_ERRO.EMAIL_JA_CADASTRADO]: "Este e-mail já está cadastrado.",
  [TIPO_ERRO.CONFLITO]: "Não foi possível concluir esta ação agora.",
  [TIPO_ERRO.SERVIDOR]: "O servidor teve um problema. Tente de novo em instantes.",
  [TIPO_ERRO.DESCONHECIDO]: "Algo deu errado. Tente de novo.",
});

/**
 * Erro padrão do módulo.
 *  - tipo: um dos valores de TIPO_ERRO
 *  - status: código HTTP (0 quando não houve resposta)
 *  - campos: nomes dos campos inválidos no 400, em camelCase (ex.: ["nome", "email"])
 *  - detalheTecnico: texto vindo da API. Nunca exibir na tela.
 */
export class ApiError extends Error {
  constructor(tipo, { status = 0, campos = [], detalheTecnico = null } = {}) {
    super(MENSAGENS[tipo] ?? MENSAGENS[TIPO_ERRO.DESCONHECIDO]);
    this.name = "ApiError";
    this.tipo = tipo;
    this.status = status;
    this.campos = campos;
    this.detalheTecnico = detalheTecnico;
  }
}

// ---------------------------------------------------------------------------
// Funções internas
// ---------------------------------------------------------------------------

function tipoPorStatus(status) {
  if (status === 400) return TIPO_ERRO.DADOS_INVALIDOS;
  if (status === 404) return TIPO_ERRO.NAO_ENCONTRADO;
  if (status === 409) return TIPO_ERRO.CONFLITO;
  if (status >= 500) return TIPO_ERRO.SERVIDOR;
  return TIPO_ERRO.DESCONHECIDO;
}

// No 400, a API devolve { errors: { Nome: [...], Email: [...] } }.
// Aproveitamos só os nomes dos campos (primeira letra minúscula, como no formulário).
function camposComErro(corpo) {
  if (!corpo || typeof corpo.errors !== "object" || corpo.errors === null) {
    return [];
  }
  return Object.keys(corpo.errors).map(
    (chave) => chave.charAt(0).toLowerCase() + chave.slice(1)
  );
}

/**
 * Faz a requisição e devolve o corpo da resposta já convertido de JSON.
 * Lança ApiError em qualquer falha.
 */
async function requisitar(caminho, opcoes = {}) {
  let resposta;

  try {
    resposta = await fetch(`${URL_BASE}${caminho}`, {
      ...opcoes,
      headers: { Accept: "application/json", ...opcoes.headers },
    });
  } catch (causa) {
    // O fetch só falha (rejeita) quando nem chegou a existir uma resposta HTTP.
    console.error("[api] sem resposta do servidor:", causa);
    throw new ApiError(TIPO_ERRO.REDE, { detalheTecnico: String(causa) });
  }

  // Corpo vazio ou que não é JSON (por exemplo, uma página de erro em HTML) vira null.
  const corpo = await resposta.json().catch(() => null);

  if (!resposta.ok) {
    const erro = new ApiError(tipoPorStatus(resposta.status), {
      status: resposta.status,
      campos: camposComErro(corpo),
      detalheTecnico: corpo?.detail ?? corpo?.title ?? null,
    });

    if (erro.tipo === TIPO_ERRO.SERVIDOR || erro.tipo === TIPO_ERRO.DESCONHECIDO) {
      console.error("[api] erro inesperado:", resposta.status, erro.detalheTecnico);
    }

    throw erro;
  }

  return corpo;
}

// ---------------------------------------------------------------------------
// Usuários
// ---------------------------------------------------------------------------

/**
 * @typedef {Object} Usuario
 * @property {number} id
 * @property {string} nome
 * @property {string} email
 * @property {string|null} areaPreferencia
 * @property {string} dataCriacao
 */

/**
 * POST /api/usuarios: cadastra um usuário.
 * Se o e-mail já existir, lança ApiError com tipo EMAIL_JA_CADASTRADO.
 * Nesse caso, quem chamou pode usar buscarUsuarioPorEmail para recuperá-lo.
 *
 * @param {{ nome: string, email: string, areaPreferencia?: string }} dados
 * @returns {Promise<Usuario>}
 */
export async function cadastrarUsuario({ nome, email, areaPreferencia }) {
  try {
    // "return await" é necessário aqui: sem o await, o catch abaixo não pegaria o erro.
    return await requisitar("/usuarios", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ nome, email, areaPreferencia }),
    });
  } catch (erro) {
    // No cadastro, um 409 só pode significar "e-mail já cadastrado".
    if (erro instanceof ApiError && erro.status === 409) {
      throw new ApiError(TIPO_ERRO.EMAIL_JA_CADASTRADO, {
        status: 409,
        detalheTecnico: erro.detalheTecnico,
      });
    }
    throw erro;
  }
}

/**
 * GET /api/usuarios/por-email?email=...: recupera um usuário existente.
 * Lança ApiError com tipo NAO_ENCONTRADO (404) se não houver usuário com o e-mail,
 * ou DADOS_INVALIDOS (400) se o e-mail for inválido.
 *
 * @param {string} email
 * @returns {Promise<Usuario>}
 */
export async function buscarUsuarioPorEmail(email) {
  // encodeURIComponent protege caracteres especiais, como o "+" e o "@".
  return requisitar(`/usuarios/por-email?email=${encodeURIComponent(email ?? "")}`);
}

// ---------------------------------------------------------------------------
// Desafios
// ---------------------------------------------------------------------------

/**
 * GET /api/desafios: lista os desafios disponíveis.
 * A API devolve o array direto na raiz. Lista vazia é um resultado válido ([]), não erro.
 *
 * @returns {Promise<Array<{ id: number, titulo: string, descricao: string, dificuldade: string, categoria: string }>>}
 */
export async function listarDesafios() {
  return requisitar("/desafios");
}

/**
 * GET /api/desafios/{id}: busca os detalhes de um desafio.
 *
 * @param {number|string} id
 * @returns {Promise<{
 *   id: number,
 *   titulo: string,
 *   descricao: string,
 *   situacaoProblema: string,
 *   dadosDisponiveis: string,
 *   objetivo: string,
 *   perguntaPrincipal: string,
 *   dificuldade: string,
 *   categoria: string
 * }>}
 */
export async function buscarDesafioPorId(id) {
  const textoId = String(id).trim();
  const numeroId = Number(textoId);

  if (
    !/^[1-9]\d*$/.test(textoId) ||
    !Number.isSafeInteger(numeroId) ||
    numeroId > 2147483647
  ) {
    throw new ApiError(TIPO_ERRO.NAO_ENCONTRADO);
  }

  return requisitar(`/desafios/${numeroId}`);
}

// ---------------------------------------------------------------------------
// Respostas
// ---------------------------------------------------------------------------

const ID_MAXIMO = 2147483647;      // maior int do C#
const LIMITE_TEXTO_RESPOSTA = 5000; // mesmo limite do CriarRespostaDto

/**
 * @typedef {Object} RespostaInicial
 * @property {number} id
 * @property {number} usuarioId
 * @property {number} desafioId
 * @property {string} analiseInicial
 * @property {string} justificativaInicial
 * @property {boolean} consultouIA
 * @property {string} dataInicio   Data em formato ISO 8601 (UTC).
 */

/**
 * POST /api/respostas: salva a análise e a justificativa iniciais de um usuário em um desafio.
 * Os textos são enviados sem espaços nas pontas. Dados inválidos nem chegam a ser enviados:
 * lança ApiError do tipo DADOS_INVALIDOS, com `campos` indicando quais campos falharam.
 *
 * @param {{ usuarioId: number, desafioId: number, analiseInicial: string, justificativaInicial: string }} dados
 * @returns {Promise<RespostaInicial>} A resposta criada, incluindo o `id` dela.
 */
export async function criarRespostaInicial({ usuarioId, desafioId, analiseInicial, justificativaInicial }) {
  const analise = typeof analiseInicial === "string" ? analiseInicial.trim() : "";
  const justificativa = typeof justificativaInicial === "string" ? justificativaInicial.trim() : "";

  const idValido = (id) => Number.isInteger(id) && id >= 1 && id <= ID_MAXIMO;
  const textoValido = (texto) => texto.length > 0 && texto.length <= LIMITE_TEXTO_RESPOSTA;

  const campos = [];
  if (!idValido(usuarioId)) campos.push("usuarioId");
  if (!idValido(desafioId)) campos.push("desafioId");
  if (!textoValido(analise)) campos.push("analiseInicial");
  if (!textoValido(justificativa)) campos.push("justificativaInicial");

  if (campos.length > 0) {
    throw new ApiError(TIPO_ERRO.DADOS_INVALIDOS, {
      campos,
      detalheTecnico: `campos inválidos: ${campos.join(", ")}`,
    });
  }

  return requisitar("/respostas", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      usuarioId,
      desafioId,
      analiseInicial: analise,
      justificativaInicial: justificativa,
    }),
  });
}

/**
 * POST /api/respostas/{respostaId}/consultar-ia: consulta a análise da IA para uma resposta já salva.
 * Não envia corpo. Se a análise já foi consultada antes, o backend devolve a que está salva.
 * Um id que não é inteiro positivo (até o maior int do C#) nem chega a ser enviado:
 * lança ApiError do tipo DADOS_INVALIDOS.
 *
 * @param {number} respostaId
 * @returns {Promise<{ respostaId: number, analiseIA: string, consultouIA: boolean }>}
 */
export async function consultarAnaliseIa(respostaId) {
  if (!Number.isInteger(respostaId) || respostaId < 1 || respostaId > ID_MAXIMO) {
    throw new ApiError(TIPO_ERRO.DADOS_INVALIDOS, {
      campos: ["respostaId"],
      detalheTecnico: "respostaId inválido",
    });
  }

  return requisitar(`/respostas/${respostaId}/consultar-ia`, { method: "POST" });
} 

// ---------------------------------------------------------------------------
// Decisão final
// ---------------------------------------------------------------------------

const DECISOES_FINAIS = Object.freeze([
  "ManterRespostaInicial",
  "ModificarComBaseNaIA"
]);

export async function finalizarResposta(
  respostaId,
  decisaoFinal,
  justificativaFinal
) {
  const justificativa =
    typeof justificativaFinal === "string"
      ? justificativaFinal.trim()
      : "";

  const campos = [];

  if (
    !Number.isInteger(respostaId) ||
    respostaId < 1 ||
    respostaId > ID_MAXIMO
  ) {
    campos.push("respostaId");
  }

  if (!DECISOES_FINAIS.includes(decisaoFinal)) {
    campos.push("decisaoFinal");
  }

  if (
    justificativa.length === 0 ||
    justificativa.length > LIMITE_TEXTO_RESPOSTA
  ) {
    campos.push("justificativaFinal");
  }

  if (campos.length > 0) {
    throw new ApiError(TIPO_ERRO.DADOS_INVALIDOS, {
      campos,
      detalheTecnico: `campos inválidos: ${campos.join(", ")}`
    });
  }

  return requisitar(`/respostas/${respostaId}/finalizar`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      decisaoFinal,
      justificativaFinal: justificativa
    })
  });
}

/**
 * POST /api/respostas/{respostaId}/avaliar: pede a avaliação automática de uma resposta já finalizada.
 * Não envia corpo. Se a resposta já foi avaliada, o backend devolve a avaliação salva, sem chamar a IA de novo.
 * Um id que não é inteiro positivo (até o maior int do C#) nem chega a ser enviado:
 * lança ApiError do tipo DADOS_INVALIDOS.
 * Outros erros: NAO_ENCONTRADO (404), CONFLITO (409, resposta ainda não finalizada),
 * SERVIDOR (5xx, inclusive 502: falha ao obter a avaliação da IA) e REDE.
 *
 * @param {number} respostaId
 * @returns {Promise<{ respostaId: number, identificacao: number, usoDosDados: number, hipotese: number, justificativa: number, pensamentoCritico: number, total: number, feedback: string }>}
 */
export async function avaliarResposta(respostaId) {
  if (!Number.isInteger(respostaId) || respostaId < 1 || respostaId > ID_MAXIMO) {
    throw new ApiError(TIPO_ERRO.DADOS_INVALIDOS, {
      campos: ["respostaId"],
      detalheTecnico: "respostaId inválido",
    });
  }

  return requisitar(`/respostas/${respostaId}/avaliar`, { method: "POST" });
}

/**
 * POST /api/respostas/{respostaId}/autoavaliacao
 * Salva a reflexão do usuário após finalizar o desafio.
 * A avaliação da IA é opcional.
 *
 * @param {number} respostaId
 * @param {{ oQueFezBem: string, oQuePodeMelhorar: string }} dados
 * @returns {Promise<{
 *   id: number,
 *   respostaId: number,
 *   oQueFezBem: string,
 *   oQuePodeMelhorar: string,
 *   dataCriacao: string
 * }>}
 */
export async function criarAutoavaliacao(respostaId, { oQueFezBem, oQuePodeMelhorar }) {
  const pontosFortes = typeof oQueFezBem === "string" ? oQueFezBem.trim() : "";
  const melhorias = typeof oQuePodeMelhorar === "string" ? oQuePodeMelhorar.trim() : "";

  const campos = [];

  if (!Number.isInteger(respostaId) || respostaId < 1 || respostaId > ID_MAXIMO) {
    campos.push("respostaId");
  }

  if (pontosFortes.length === 0 || pontosFortes.length > 2000) {
    campos.push("oQueFezBem");
  }

  if (melhorias.length === 0 || melhorias.length > 2000) {
    campos.push("oQuePodeMelhorar");
  }

  if (campos.length > 0) {
    throw new ApiError(TIPO_ERRO.DADOS_INVALIDOS, {
      campos,
      detalheTecnico: `campos inválidos: ${campos.join(", ")}`
    });
  }

  return requisitar(`/respostas/${respostaId}/autoavaliacao`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      oQueFezBem: pontosFortes,
      oQuePodeMelhorar: melhorias
    })
  });
}

/**
 * GET /api/respostas/{respostaId}/autoavaliacao
 * Recupera uma autoavaliação já registrada no PostgreSQL.
 *
 * Se não existir, lança ApiError com status 404.
 *
 * @param {number} respostaId
 * @returns {Promise<{
 *   id: number,
 *   respostaId: number,
 *   oQueFezBem: string,
 *   oQuePodeMelhorar: string,
 *   dataCriacao: string
 * }>}
 */
export async function buscarAutoavaliacao(respostaId) {
  if (
    !Number.isInteger(respostaId) ||
    respostaId < 1 ||
    respostaId > ID_MAXIMO
  ) {
    throw new ApiError(TIPO_ERRO.DADOS_INVALIDOS, {
      campos: ["respostaId"],
      detalheTecnico: "respostaId inválido"
    });
  }

  return requisitar(`/respostas/${respostaId}/autoavaliacao`);
}