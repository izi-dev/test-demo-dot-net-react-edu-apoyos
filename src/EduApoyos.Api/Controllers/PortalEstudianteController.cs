using System.Text;
using EduApoyos.Api.Auth;
using EduApoyos.Application.Features.Constancias;
using EduApoyos.Application.Features.Solicitudes;
using EduApoyos.Application.Features.Solicitudes.ListByEstudiante;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// Controlador delgado del portal del estudiante.
/// </summary>
[ApiController]
[Authorize(Roles = "Estudiante")]
[Route("api/estudiantes")]
public sealed class PortalEstudianteController(
    ListSolicitudesByEstudianteQueryHandler listByEstudianteHandler,
    ListMisSolicitudesQueryHandler listMisSolicitudesHandler,
    GenerateConstanciaQueryHandler constanciaHandler) : ControllerBase
{
    [HttpGet("{id:guid}/solicitudes")]
    [ProducesResponseType<IReadOnlyCollection<SolicitudDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<SolicitudDto>> List(Guid id, CancellationToken cancellationToken) =>
        listByEstudianteHandler.HandleAsync(
            new ListSolicitudesByEstudianteQuery(id, User.GetUserId()),
            cancellationToken);

    [HttpGet("me/solicitudes")]
    [ProducesResponseType<IReadOnlyCollection<SolicitudDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<SolicitudDto>> ListMine(CancellationToken cancellationToken) =>
        listMisSolicitudesHandler.HandleAsync(
            new ListMisSolicitudesQuery(User.GetUserId()),
            cancellationToken);

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
