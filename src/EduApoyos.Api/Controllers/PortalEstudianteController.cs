using System.Text;
using EduApoyos.Api.Auth;
using EduApoyos.Application.Features.Constancias;
using EduApoyos.Application.Features.Solicitudes;
using EduApoyos.Application.Features.Solicitudes.ListByEstudiante;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// Endpoints del portal del estudiante: listado de solicitudes propias y descarga de constancia.
/// </summary>
[ApiController]
[Authorize(Roles = "Estudiante")]
[Route("api/estudiantes")]
public sealed class PortalEstudianteController(
    ListSolicitudesByEstudianteQueryHandler listByEstudianteHandler,
    ListMisSolicitudesQueryHandler listMisSolicitudesHandler,
    GenerateConstanciaQueryHandler constanciaHandler) : ControllerBase
{
    /// <summary>
    /// Lista las solicitudes de un estudiante concreto (el autenticado debe ser el dueño).
    /// </summary>
    /// <param name="id">Identificador del estudiante.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Colección de solicitudes del estudiante.</returns>
    /// <remarks>
    /// Roles requeridos: Estudiante.
    /// Códigos de respuesta:
    /// - 200: listado de solicitudes.
    /// - 401: sin autenticación o token inválido.
    /// - 403: el usuario no es el dueño del perfil o no tiene rol Estudiante.
    /// - 404: estudiante no encontrado.
    /// - 500: error interno (vía middleware).
    /// </remarks>
    [HttpGet("{id:guid}/solicitudes")]
    [ProducesResponseType<IReadOnlyCollection<SolicitudDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<SolicitudDto>> List(Guid id, CancellationToken cancellationToken) =>
        listByEstudianteHandler.HandleAsync(
            new ListSolicitudesByEstudianteQuery(id, User.GetUserId()),
            cancellationToken);

    /// <summary>
    /// Lista las solicitudes del estudiante autenticado (atajo <c>/me</c>).
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Colección de solicitudes del usuario actual.</returns>
    /// <remarks>
    /// Roles requeridos: Estudiante.
    /// Códigos de respuesta:
    /// - 200: listado de solicitudes propias.
    /// - 401: sin autenticación o token inválido.
    /// - 403: autenticado pero sin rol Estudiante.
    /// - 404: perfil de estudiante no encontrado para el usuario.
    /// - 500: error interno (vía middleware).
    /// </remarks>
    [HttpGet("me/solicitudes")]
    [ProducesResponseType<IReadOnlyCollection<SolicitudDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<SolicitudDto>> ListMine(CancellationToken cancellationToken) =>
        listMisSolicitudesHandler.HandleAsync(
            new ListMisSolicitudesQuery(User.GetUserId()),
            cancellationToken);

    /// <summary>
    /// Descarga la constancia en texto plano de una solicitud del estudiante.
    /// </summary>
    /// <param name="id">Identificador del estudiante.</param>
    /// <param name="solicitudId">Identificador de la solicitud.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Archivo de texto con la constancia.</returns>
    /// <remarks>
    /// Roles requeridos: Estudiante.
    /// Content-Type: text/plain.
    /// Códigos de respuesta:
    /// - 200: archivo de constancia.
    /// - 401: sin autenticación o token inválido.
    /// - 403: acceso denegado (no es el dueño).
    /// - 404: estudiante o solicitud no encontrados.
    /// - 400: regla de negocio (p. ej. estado no apto para constancia).
    /// - 500: error interno (vía middleware).
    /// </remarks>
    [HttpGet("{id:guid}/solicitudes/{solicitudId:guid}/constancia")]
    [Produces("text/plain")]
    public async Task<IActionResult> DescargarConstancia(Guid id, Guid solicitudId, CancellationToken cancellationToken)
    {
        var resultado = await constanciaHandler.HandleAsync(
            new GenerateConstanciaQuery(id, solicitudId, User.GetUserId()),
            cancellationToken);

        return File(
            Encoding.UTF8.GetBytes(resultado.Contenido),
            resultado.ContentType,
            resultado.NombreArchivo);
    }
}
