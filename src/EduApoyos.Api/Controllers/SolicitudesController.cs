using EduApoyos.Api.Auth;
using EduApoyos.Application.Common;
using EduApoyos.Application.Features.Solicitudes;
using EduApoyos.Application.Features.Solicitudes.ChangeStatus;
using EduApoyos.Application.Features.Solicitudes.Create;
using EduApoyos.Application.Features.Solicitudes.GetById;
using EduApoyos.Application.Features.Solicitudes.List;
using EduApoyos.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduApoyos.Api.Controllers;

/// <summary>
/// Controlador delgado para solicitudes de apoyo económico.
/// </summary>
[ApiController]
[Route("api/solicitudes")]
[Authorize]
public sealed class SolicitudesController(
    ListSolicitudesQueryHandler listHandler,
    CreateSolicitudCommandHandler createHandler,
    GetSolicitudByIdQueryHandler getByIdHandler,
    ChangeSolicitudStatusCommandHandler changeStatusHandler) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Asesor")]
    [ProducesResponseType<PagedResult<SolicitudDto>>(StatusCodes.Status200OK)]
    public Task<PagedResult<SolicitudDto>> List(
        [FromQuery] EstadoSolicitud? estado,
        [FromQuery] TipoApoyo? tipo,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        listHandler.HandleAsync(
            new ListSolicitudesQuery(estado, tipo, desde, hasta, page, pageSize),
            cancellationToken);

    [HttpPost]
    [Authorize(Roles = "Asesor,Estudiante")]
    [ProducesResponseType<SolicitudDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<SolicitudDto>> Create(CreateSolicitudRequest request, CancellationToken cancellationToken)
    {
        var result = await createHandler.HandleAsync(
            new CreateSolicitudCommand(
                EstudianteId: request.EstudianteId,
                TipoApoyo: request.TipoApoyo,
                MontoSolicitado: request.MontoSolicitado,
                Descripcion: request.Descripcion,
                UsuarioSolicitanteId: User.GetUserId(),
                RolSolicitante: User.GetRol()),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Asesor,Estudiante")]
    [ProducesResponseType<SolicitudDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SolicitudDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await getByIdHandler.HandleAsync(
            new GetSolicitudByIdQuery(id, User.GetUserId(), User.GetRol()),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:guid}/estado")]
    [Authorize(Roles = "Asesor")]
    [ProducesResponseType<SolicitudDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SolicitudDto>> CambiarEstado(
        Guid id,
        ChangeSolicitudStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await changeStatusHandler.HandleAsync(
            new ChangeSolicitudStatusCommand(id, request.Estado, request.Observacion, User.GetUserId()),
            cancellationToken);

        return Ok(result);
    }
}

public sealed record CreateSolicitudRequest(
    Guid? EstudianteId,
    TipoApoyo TipoApoyo,
    decimal MontoSolicitado,
    string Descripcion);

public sealed record ChangeSolicitudStatusRequest(
    EstadoSolicitud Estado,
    string? Observacion);
