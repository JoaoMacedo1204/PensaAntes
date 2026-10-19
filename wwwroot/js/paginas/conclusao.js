import { buscarAutoavaliacao } from "../api.js";

const conteudo = document.getElementById("conteudo-conclusao");
const estado = document.getElementById("estado-conclusao");
const mensagem = document.getElementById("mensagem-estado-conclusao");

async function iniciarConclusao() {
  const parametros = new URLSearchParams(window.location.search);
  const valorId = parametros.get("respostaId");

  // Aceita apenas IDs inteiros positivos dentro do limite do banco.
  if (!valorId || !/^[1-9]\d*$/.test(valorId)) {
    mostrarErro("Não foi possível identificar o desafio concluído.");
    return;
  }

  const respostaId = Number(valorId);

  if (!Number.isSafeInteger(respostaId) || respostaId > 2147483647) {
    mostrarErro("O identificador do desafio é inválido.");
    return;
  }

  try {
    const autoavaliacao = await buscarAutoavaliacao(respostaId);

    if (
      !autoavaliacao ||
      autoavaliacao.respostaId !== respostaId ||
      !Number.isInteger(autoavaliacao.id) ||
      autoavaliacao.id <= 0
    ) {
      mostrarErro("Não foi possível confirmar a conclusão do desafio.");
      return;
    }

    estado.hidden = true;
    conteudo.hidden = false;

    document.getElementById("titulo-conclusao").focus();

  } catch (erro) {
    console.error("[conclusao] Falha na verificação:", erro);

    mostrarErro(
      "Não foi possível confirmar sua conclusão. Verifique sua conexão e tente novamente."
    );
  }
}

function mostrarErro(texto) {
  mensagem.textContent = texto;
  estado.classList.add("estado--erro");
  conteudo.hidden = true;
}

iniciarConclusao();
