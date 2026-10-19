using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PensaAntes.Api.Data;
using PensaAntes.Api.DTOs.Usuarios;
using PensaAntes.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace PensaAntes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsuariosController(AppDbContext context)
    {
        _context = context;
    }

    // POST /api/usuarios
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioDto>> Criar(
        CriarUsuarioDto dto,
        CancellationToken cancellationToken)
    {
        var nome = dto.Nome.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();
        var areaPreferencia = string.IsNullOrWhiteSpace(dto.AreaPreferencia)
            ? null
            : dto.AreaPreferencia.Trim();

        var emailJaCadastrado = await _context.Usuarios
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailJaCadastrado)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "E-mail já cadastrado",
                Detail = "Já existe um usuário cadastrado com este e-mail."
            });
        }

        var usuario = new Usuario
        {
            Nome = nome,
            Email = email,
            AreaPreferencia = areaPreferencia
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(cancellationToken);

        var resposta = new UsuarioDto
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            AreaPreferencia = usuario.AreaPreferencia,
            DataCriacao = usuario.DataCriacao
        };

        return StatusCode(StatusCodes.Status201Created, resposta);
    }


    // Validação do usuário pelo e-mail
    // GET /api/usuarios/por-email
    [HttpGet("por-email")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioDto>> BuscarPorEmail(
        [FromQuery] string? email,
        CancellationToken cancellationToken)
    {
        // Mesma normalização do cadastro: sem espaços nas pontas e em minúsculas.
        var emailNormalizado = email?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(emailNormalizado)
            || emailNormalizado.Length > 254
            || !new EmailAddressAttribute().IsValid(emailNormalizado))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(email)] = new[] { "Informe um e-mail válido." }
            }));
        }

        var usuario = await _context.Usuarios
            .Where(u => u.Email == emailNormalizado)
            .Select(u => new UsuarioDto
            {
                Id = u.Id,
                Nome = u.Nome,
                Email = u.Email,
                AreaPreferencia = u.AreaPreferencia,
                DataCriacao = u.DataCriacao
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (usuario is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Usuário não encontrado",
                Detail = "Não existe usuário cadastrado com este e-mail."
            });
        }

        return Ok(usuario);
    }
}