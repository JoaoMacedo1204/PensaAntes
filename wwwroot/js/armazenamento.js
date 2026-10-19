/**
 * armazenamento.js: guarda no navegador quem é o usuário identificado,
 * para as outras páginas saberem o `id` dele sem pedir o cadastro de novo.
 *
 * Atenção: isto NÃO é autenticação nem segurança. O localStorage pode ser lido e
 * editado por qualquer pessoa que use o navegador (inclusive pelas ferramentas
 * de desenvolvedor). Por isso só guardamos o mínimo (id, nome e área) e nunca
 * e-mail, senha ou qualquer dado sensível.
 */

const CHAVE_USUARIO = "pensaAntes.usuario";

/**
 * @typedef {Object} UsuarioSalvo
 * @property {number} id
 * @property {string} nome
 * @property {string|null} areaPreferencia
 */

/**
 * Confere o formato e devolve um objeto novo só com os campos permitidos,
 * ou null se os dados não servirem. Qualquer outro campo (como o e-mail
 * que vem da API) é descartado aqui.
 */
function normalizarUsuario(dados) {
  if (!dados || typeof dados !== "object") return null;

  const { id, nome, areaPreferencia } = dados;

  if (!Number.isInteger(id) || id <= 0) return null;
  if (typeof nome !== "string" || nome.trim() === "") return null;

  const area =
    typeof areaPreferencia === "string" && areaPreferencia.trim() !== ""
      ? areaPreferencia.trim()
      : null;

  return { id, nome: nome.trim(), areaPreferencia: area };
}

/**
 * Salva o usuário identificado.
 * Aceita o objeto completo devolvido pela API: só id, nome e areaPreferencia são guardados.
 *
 * @param {{ id: number, nome: string, areaPreferencia?: string|null }} usuario
 * @returns {boolean} true se salvou; false se os dados são inválidos ou o navegador não permitiu salvar.
 */
export function salvarUsuario(usuario) {
  const dados = normalizarUsuario(usuario);

  if (!dados) {
    console.warn("[armazenamento] usuário inválido, nada foi salvo.");
    return false;
  }

  try {
    // O localStorage só guarda texto, então o objeto vira texto com JSON.stringify.
    localStorage.setItem(CHAVE_USUARIO, JSON.stringify(dados));
    return true;
  } catch (erro) {
    // Pode falhar se o navegador bloqueia o armazenamento (modo privado, configurações).
    console.warn("[armazenamento] não foi possível salvar no navegador:", erro);
    return false;
  }
}

/**
 * Recupera o usuário salvo.
 *
 * @returns {UsuarioSalvo|null} null se não há usuário salvo ou se o que estava salvo é inválido.
 */
export function recuperarUsuario() {
  let texto;

  try {
    texto = localStorage.getItem(CHAVE_USUARIO);
  } catch {
    return null; // armazenamento indisponível
  }

  if (texto === null) return null; // nada salvo ainda

  try {
    // O que está guardado é texto, então JSON.parse transforma de volta em objeto.
    const usuario = normalizarUsuario(JSON.parse(texto));
    if (usuario) return usuario;
  } catch {
    // JSON quebrado: cai no descarte logo abaixo.
  }

  // Dado corrompido ou alterado à mão: descarta para não quebrar as páginas.
  removerUsuario();
  return null;
}

/**
 * Remove o usuário salvo (por exemplo, para "trocar de usuário").
 */
export function removerUsuario() {
  try {
    localStorage.removeItem(CHAVE_USUARIO);
  } catch {
    // Se o armazenamento está indisponível, não há o que remover.
  }
}