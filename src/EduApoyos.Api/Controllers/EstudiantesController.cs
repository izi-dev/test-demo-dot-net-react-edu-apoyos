using EduApoyos.Application.Common;
using EduApoyos.Application.Features.Estudiantes;
using EduApoyos.Application.Features.Estudiantes.Create;
using EduApoyos.Application.Features.Estudiantes.List;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// Controlador delgado para operaciones de estudiantes del asesor.
/// </summary>
[ApiController]
[Route("api/estudiantes")]
[Authorize(Roles = "Asesor")]
public sealed class EstudiantesController(
    ListEstudiantesQueryHandler listHandler,
    CreateEstudianteCommandHandler createHandler) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<EstudianteDto>>(StatusCodes.Status200OK)]
    public Task<PagedResult<EstudianteDto>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        listHandler.HandleAsync(new ListEstudiantesQuery(page, pageSize), cancellationToken);

    [HttpPost]
    [ProducesResponseType<EstudianteDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<EstudianteDto>> Create(CreateEstudianteRequest request, CancellationToken cancellationToken)
    {
        var result = await createHandler.HandleAsync(
            new CreateEstudianteCommand(
                UsuarioId: request.UsuarioId,
                NumeroDocumento: request.NumeroDocumento,
                TipoDocumento: request.TipoDocumento,
                ProgramaAcademico: request.ProgramaAcademico,
                Semestre: request.Semestre),
            cancellationToken);

        return CreatedAtAction(nameof(List), new { id = result.Id }, result);
    }
}

/// <summary>
/// Contrato HTTP para crear un estudiante.
/// </summary>
public sealed record CreateEstudianteRequest(
    Guid UsuarioId,
    string NumeroDocumento,
    Domain.Enums.TipoDocumento TipoDocumento,
    string ProgramaAcademico,
    int Semestre);
