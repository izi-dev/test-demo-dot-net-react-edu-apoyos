using EduApoyos.Application.Common;
using EduApoyos.Application.Features.Estudiantes;
using EduApoyos.Application.Features.Estudiantes.Create;
using EduApoyos.Application.Features.Estudiantes.List;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// Endpoints de gestión de estudiantes destinados al rol Asesor.
/// </summary>
[ApiController]
[Route("api/estudiantes")]
[Authorize(Roles = "Asesor")]
public sealed class EstudiantesController(
    ListEstudiantesQueryHandler listHandler,
    CreateEstudianteCommandHandler createHandler) : ControllerBase
{
    /// <summary>
    /// Lista estudiantes de forma paginada.
    /// </summary>
    /// <param name="page">Número de página (1-based). Valor por defecto: 1.</param>
    /// <param name="pageSize">Tamaño de página. Valor por defecto: 10.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado paginado de estudiantes.</returns>
    /// <remarks>
    /// Roles requeridos: Asesor (JWT Bearer).
    /// Códigos de respuesta:
    /// - 200: listado paginado.
    /// - 401: sin autenticación o token inválido.
    /// - 403: autenticado pero sin rol Asesor.
    /// - 500: error interno (vía middleware).
    /// </remarks>
    [HttpGet]
    [ProducesResponseType<PagedResult<EstudianteDto>>(StatusCodes.Status200OK)]
    public Task<PagedResult<EstudianteDto>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        listHandler.HandleAsync(new ListEstudiantesQuery(page, pageSize), cancellationToken);

    /// <summary>
    /// Crea un perfil de estudiante vinculado a un usuario existente.
    /// </summary>
    /// <param name="request">Datos del estudiante a crear.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Estudiante creado, con ubicación en el encabezado Location.</returns>
    /// <remarks>
    /// Roles requeridos: Asesor (JWT Bearer).
    /// Códigos de respuesta:
    /// - 201: estudiante creado.
    /// - 400: validación o regla de negocio (vía middleware).
    /// - 401: sin autenticación o token inválido.
    /// - 403: autenticado pero sin rol Asesor.
    /// - 404: usuario referenciado no encontrado (vía middleware).
    /// - 500: error interno (vía middleware).
    /// </remarks>
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
/// <param name="UsuarioId">Identificador del usuario de dominio al que se asocia el perfil.</param>
/// <param name="NumeroDocumento">Número de documento de identidad (único).</param>
/// <param name="TipoDocumento">Tipo de documento (enum de dominio).</param>
/// <param name="ProgramaAcademico">Nombre del programa académico.</param>
/// <param name="Semestre">Semestre actual del estudiante.</param>
public sealed record CreateEstudianteRequest(
    Guid UsuarioId,
    string NumeroDocumento,
    Domain.Enums.TipoDocumento TipoDocumento,
    string ProgramaAcademico,
    int Semestre);
