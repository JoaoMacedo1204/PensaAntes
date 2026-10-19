using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PensaAntes.Api.Data;
using PensaAntes.Api.DTOs.Desafios;

namespace PensaAntes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DesafiosController : ControllerBase
{
    private readonly AppDbContext _context;

    public DesafiosController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/desafios
    [HttpGet]
    public async Task<ActionResult<List<DesafioResumoDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var desafios = await _context.Desafios
            .AsNoTracking()
            .OrderBy(d => d.Id)
            .Select(d => new DesafioResumoDto
            {
                Id = d.Id,
                Titulo = d.Titulo,
                Descricao = d.Descricao,
                Dificuldade = d.Dificuldade,
                Categoria = d.Categoria.Nome
            })
            .ToListAsync(cancellationToken);

        return Ok(desafios);
    }

    // GET /api/desafios/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DesafioDetalheDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var desafio = await _context.Desafios
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DesafioDetalheDto
            {
                Id = d.Id,
                Titulo = d.Titulo,
                Descricao = d.Descricao,
                SituacaoProblema = d.SituacaoProblema,
                DadosDisponiveis = d.DadosDisponiveis,
                Objetivo = d.Objetivo,
                PerguntaPrincipal = d.PerguntaPrincipal,
                Dificuldade = d.Dificuldade,
                Categoria = d.Categoria.Nome
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (desafio is null)
        {
            return NotFound();
        }

        return Ok(desafio);
    }
}