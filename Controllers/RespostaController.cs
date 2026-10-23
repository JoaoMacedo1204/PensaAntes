using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PensaAntes.Api.Data;
using PensaAntes.Api.DTOs.Respostas;
using PensaAntes.Api.Models;
using PensaAntes.Api.Services;
using PensaAntes.Api.Enums;

namespace PensaAntes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RespostasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IGeminiService _geminiService;
    private readonly IAvaliacaoService _avaliacaoService;
    private readonly ILogger<RespostasController> _logger;

    public RespostasController(
        AppDbContext context,
        IGeminiService geminiService,
        IAvaliacaoService avaliacaoService,
        ILogger<RespostasController> logger)
    {
        _context = context;
        _geminiService = geminiService;
        _avaliacaoService = avaliacaoService;
        _logger = logger;
    }

    // POST /api/respostas
    [HttpPost]
    [ProducesResponseType(typeof(RespostaInicialDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RespostaInicialDto>> Criar(
        CriarRespostaDto dto,
        CancellationToken cancellationToken)
    {
        var analiseInicial = dto.AnaliseInicial.Trim();
        var justificativaInicial = dto.JustificativaInicial.Trim();

        if (analiseInicial.Length == 0 || justificativaInicial.Length == 0)
        {
            var erros = new Dictionary<string, string[]>();

            if (analiseInicial.Length == 0)
            {
                erros[nameof(dto.AnaliseInicial)] = new[] { "A análise inicial não pode ficar vazia." };
            }

            if (justificativaInicial.Length == 0)
            {
                erros[nameof(dto.JustificativaInicial)] = new[] { "A justificativa inicial não pode ficar vazia." };
            }

            return ValidationProblem(new ValidationProblemDetails(erros));
        }

        var usuarioExiste = await _context.Usuarios
            .AsNoTracking()
            .AnyAsync(u => u.Id == dto.UsuarioId, cancellationToken);

        if (!usuarioExiste)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Usuário não encontrado",
                Detail = $"Não existe usuário com o Id {dto.UsuarioId}."
            });
        }

        var desafioExiste = await _context.Desafios
            .AsNoTracking()
            .AnyAsync(d => d.Id == dto.DesafioId, cancellationToken);

        if (!desafioExiste)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Desafio não encontrado",
                Detail = $"Não existe desafio com o Id {dto.DesafioId}."
            });
        }

        var resposta = new Resposta
        {
            UsuarioId = dto.UsuarioId,
            DesafioId = dto.DesafioId,
            AnaliseInicial = analiseInicial,
            JustificativaInicial = justificativaInicial,
            ConsultouIA = false,
            DataInicio = DateTime.UtcNow
        };

        _context.Respostas.Add(resposta);
        await _context.SaveChangesAsync(cancellationToken);

        var resultado = new RespostaInicialDto
        {
            Id = resposta.Id,
            UsuarioId = resposta.UsuarioId,
            DesafioId = resposta.DesafioId,
            AnaliseInicial = resposta.AnaliseInicial,
            JustificativaInicial = resposta.JustificativaInicial,
            ConsultouIA = resposta.ConsultouIA,
            DataInicio = resposta.DataInicio
        };

        return StatusCode(StatusCodes.Status201Created, resultado);
    }

    // POST /api/respostas/{id}/consultar-ia
    [HttpPost("{id:int}/consultar-ia")]
    [ProducesResponseType(typeof(AnaliseIaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<AnaliseIaDto>> ConsultarIa(
        int id,
        CancellationToken cancellationToken)
    {
        // Com rastreamento (sem AsNoTracking): vamos alterar e salvar esta entidade.
        var resposta = await _context.Respostas
            .Include(r => r.Desafio)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (resposta is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resposta não encontrada",
                Detail = $"Não existe resposta com o Id {id}."
            });
        }

        // Já consultou: devolve a análise salva, sem nova chamada ao Gemini.
        if (resposta.ConsultouIA)
        {
            return Ok(new AnaliseIaDto
            {
                RespostaId = resposta.Id,
                AnaliseIA = resposta.AnaliseIA ?? string.Empty,
                ConsultouIA = true
            });
        }

        string analise;

        try
        {
            analise = await _geminiService.GerarAnaliseAsync(
                resposta.Desafio, resposta, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "Erro ao consultar o Gemini para a resposta {RespostaId}",
                id);

            return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = "Falha ao consultar a IA",
                Detail = "A IA está temporariamente indisponível. Aguarde alguns instantes e tente novamente. Sua análise inicial foi preservada."
            });
}

        resposta.AnaliseIA = analise;
        resposta.ConsultouIA = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new AnaliseIaDto
        {
            RespostaId = resposta.Id,
            AnaliseIA = resposta.AnaliseIA,
            ConsultouIA = resposta.ConsultouIA
        });
    }

    // PUT /api/respostas/{id}/finalizar
    [HttpPut("{id:int}/finalizar")]
    [ProducesResponseType(typeof(RespostaFinalizadaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespostaFinalizadaDto>> Finalizar(
        int id,
        FinalizarRespostaDto dto,
        CancellationToken cancellationToken)
    {
        var justificativaFinal = dto.JustificativaFinal.Trim();

        if (justificativaFinal.Length == 0)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(dto.JustificativaFinal)] = new[] { "A justificativa final não pode ficar vazia." }
            }));
        }

        // Com rastreamento: a entidade será alterada e salva.
        var resposta = await _context.Respostas
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (resposta is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resposta não encontrada",
                Detail = $"Não existe resposta com o Id {id}."
            });
        }

        if (resposta.DataFinalizacao is not null)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Resposta já finalizada",
                Detail = "Esta resposta já foi finalizada e não pode ser finalizada novamente."
            });
        }

        if (dto.DecisaoFinal == TipoDecisaoFinal.ModificarComBaseNaIA && !resposta.ConsultouIA)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(dto.DecisaoFinal)] = new[]
                {
                    "Só é possível modificar a resposta com base na IA depois de consultar a IA."
                }
            }));
        }

        var dataFinalizacao = DateTime.UtcNow;

        resposta.DecisaoFinal = dto.DecisaoFinal;
        resposta.JustificativaFinal = justificativaFinal;
        resposta.DataFinalizacao = dataFinalizacao;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new RespostaFinalizadaDto
        {
            Id = resposta.Id,
            DecisaoFinal = dto.DecisaoFinal,
            JustificativaFinal = justificativaFinal,
            ConsultouIA = resposta.ConsultouIA,
            DataFinalizacao = dataFinalizacao
        });
    }

        // POST /api/respostas/{id}/avaliar
        [HttpPost("{id:int}/avaliar")]
        [ProducesResponseType(typeof(AvaliacaoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<AvaliacaoDto>> Avaliar(
            int id,
            CancellationToken cancellationToken)
        {
            // Com rastreamento: vamos adicionar uma Avaliacao relacionada a esta Resposta.
            var resposta = await _context.Respostas
                .Include(r => r.Desafio)
                .Include(r => r.Avaliacao)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (resposta is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resposta não encontrada",
                    Detail = $"Não existe resposta com o Id {id}."
                });
            }

            if (resposta.DataFinalizacao is null)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Resposta ainda não finalizada",
                    Detail = "A resposta precisa ser finalizada antes de ser avaliada."
                });
            }

            // Já avaliada: devolve a avaliação salva, sem nova chamada ao Gemini.
            if (resposta.Avaliacao is not null)
            {
                return Ok(MapearAvaliacao(resposta.Avaliacao));
            }

            ResultadoAvaliacao resultado;

            try
            {
                resultado = await _avaliacaoService.AvaliarAsync(
                    resposta.Desafio, resposta, cancellationToken);
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(
                    ex,
                    "Erro ao gerar avaliação da IA para a resposta {RespostaId}",
                    id);

                return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
                {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "Falha ao avaliar a resposta",
                    Detail = "Não foi possível gerar sua avaliação agora. Tente novamente em alguns instantes. Sua resposta final foi preservada."
                });
            }

            var avaliacao = new Avaliacao
            {
                RespostaId = resposta.Id,
                Identificacao = resultado.Identificacao,
                UsoDosDados = resultado.UsoDosDados,
                Hipotese = resultado.Hipotese,
                Justificativa = resultado.Justificativa,
                PensamentoCritico = resultado.PensamentoCritico,
                Feedback = resultado.Feedback,
                DataAvaliacao = DateTime.UtcNow
            };

            _context.Avaliacoes.Add(avaliacao);
            await _context.SaveChangesAsync(cancellationToken);

            return Ok(MapearAvaliacao(avaliacao));
        }

        private static AvaliacaoDto MapearAvaliacao(Avaliacao avaliacao)
        {
            return new AvaliacaoDto
            {
                RespostaId = avaliacao.RespostaId,
                Identificacao = avaliacao.Identificacao,
                UsoDosDados = avaliacao.UsoDosDados,
                Hipotese = avaliacao.Hipotese,
                Justificativa = avaliacao.Justificativa,
                PensamentoCritico = avaliacao.PensamentoCritico,
                Total = avaliacao.Identificacao
                    + avaliacao.UsoDosDados
                    + avaliacao.Hipotese
                    + avaliacao.Justificativa
                    + avaliacao.PensamentoCritico,
                Feedback = avaliacao.Feedback
            };
        }


        /// <summary>
        /// Busca a autoavaliação registrada para uma resposta.
        /// </summary>
        // GET /api/respostas/{id}/autoavaliacao
        [HttpGet("{id:int}/autoavaliacao")]
        [ProducesResponseType(typeof(AutoavaliacaoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AutoavaliacaoDto>> BuscarAutoAvaliacao(
            int id,
            CancellationToken cancellationToken)
        {
            var autoavaliacao = await _context.AutoAvaliacoes
                .AsNoTracking()
                .Where(a => a.RespostaId == id)
                .Select(a => new AutoavaliacaoDto
                {
                    Id = a.Id,
                    RespostaId = a.RespostaId,
                    OQueFezBem = a.OQueFezBem,
                    OQuePodeMelhorar = a.OQuePodeMelhorar,
                    DataCriacao = a.DataCriacao
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (autoavaliacao is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Autoavaliação não encontrada",
                    Detail = "Esta resposta ainda não possui uma autoavaliação registrada."
                });
            }

            return Ok(autoavaliacao);
        }

        // POST /api/respostas/{id}/autoavaliacao
        [HttpPost("{id:int}/autoavaliacao")]
        [ProducesResponseType(typeof(AutoavaliacaoDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AutoavaliacaoDto>> CriarAutoAvaliacao(
            int id,
            CriarAutoavaliacaoDto dto,
            CancellationToken cancellationToken)
        {
            var oQueFezBem = dto.OQueFezBem.Trim();
            var oQuePodeMelhorar = dto.OQuePodeMelhorar.Trim();

            if (oQueFezBem.Length == 0 || oQuePodeMelhorar.Length == 0)
            {
                var erros = new Dictionary<string, string[]>();

                if (oQueFezBem.Length == 0)
                {
                    erros[nameof(dto.OQueFezBem)] = new[] { "O campo \"o que fez bem\" não pode ficar vazio." };
                }

                if (oQuePodeMelhorar.Length == 0)
                {
                    erros[nameof(dto.OQuePodeMelhorar)] = new[] { "O campo \"o que pode melhorar\" não pode ficar vazio." };
                }

                return ValidationProblem(new ValidationProblemDetails(erros));
            }

            // Só o estado da resposta: não carrega textos nem a análise da IA.
            var estado = await _context.Respostas
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    Finalizada = r.DataFinalizacao != null,
                    JaAutoAvaliada = r.AutoAvaliacao != null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (estado is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resposta não encontrada",
                    Detail = $"Não existe resposta com o Id {id}."
                });
            }

            if (!estado.Finalizada)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Resposta ainda não finalizada",
                    Detail = "A resposta precisa ser finalizada antes da autoavaliação."
                });
            }

            if (estado.JaAutoAvaliada)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Autoavaliação já registrada",
                    Detail = "Esta resposta já possui uma autoavaliação."
                });
            }

            var autoAvaliacao = new AutoAvaliacao
            {
                RespostaId = id,
                OQueFezBem = oQueFezBem,
                OQuePodeMelhorar = oQuePodeMelhorar,
                DataCriacao = DateTime.UtcNow
            };

            _context.AutoAvaliacoes.Add(autoAvaliacao);
            await _context.SaveChangesAsync(cancellationToken);

            var resultado = new AutoavaliacaoDto
        {
            Id = autoAvaliacao.Id,
            RespostaId = autoAvaliacao.RespostaId,
            OQueFezBem = autoAvaliacao.OQueFezBem,
            OQuePodeMelhorar = autoAvaliacao.OQuePodeMelhorar,
            DataCriacao = autoAvaliacao.DataCriacao
        };

        return StatusCode(StatusCodes.Status201Created, resultado);
    }
}