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
/// Endpoints de solicitudes de apoyo económico (listado, creación, detalle y cambio de estado).
/// Requiere autenticación JWT en todos los métodos; los roles se restringen por acción.
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
    /// <summary>
    /// Lista solicitudes con filtros opcionales y paginación.
    /// </summary>
    /// <param name="estado">Filtro opcional por estado de la solicitud.</param>
    /// <param name="tipo">Filtro opcional por tipo de apoyo.</param>
    /// <param name="desde">Fecha mínima de solicitud (inclusive).</param>
    /// <param name="hasta">Fecha máxima de solicitud (inclusive).</param>
    /// <param name="page">Número de página (1-based). Valor por defecto: 1.</param>
    /// <param name="pageSize">Tamaño de página. Valor por defecto: 10.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado paginado de solicitudes.</returns>
    /// <remarks>
    /// Roles requeridos: Asesor.
    /// Códigos de respuesta:
    /// - 200: listado paginado.
    /// - 401: sin autenticación o token inválido.
    /// - 403: autenticado pero sin rol Asesor.
    /// - 500: error interno (vía middleware).
    /// </remarks>
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

    /// <summary>
    /// Crea una nueva solicitud de apoyo.
    /// </summary>
    /// <param name="request">Datos de la solicitud (estudiante opcional, tipo, monto y descripción).</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Solicitud creada, con ubicación en el encabezado Location.</returns>
    /// <remarks>
    /// Roles requeridos: Asesor o Estudiante.
    /// El estudiante se toma del body (asesor) o del usuario autenticado (estudiante),
    /// según la lógica del caso de uso.
    /// Códigos de respuesta:
    /// - 201: solicitud creada.
    /// - 400: validación o regla de negocio (vía middleware).
    /// - 401: sin autenticación o token inválido.
    /// - 403: autenticado sin rol permitido, o acceso denegado al recurso.
    /// - 404: estudiante no encontrado (vía middleware).
    /// - 500: error interno (vía middleware).
    /// </remarks>
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

    /// <summary>
    /// Obtiene el detalle de una solicitud por identificador.
    /// </summary>
    /// <param name="id">Identificador de la solicitud.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Detalle de la solicitud.</returns>
    /// <remarks>
    /// Roles requeridos: Asesor o Estudiante.
    /// Un estudiante solo puede ver sus propias solicitudes (regla de aplicación).
    /// Códigos de respuesta:
    /// - 200: solicitud encontrada.
    /// - 401: sin autenticación o token inválido.
    /// - 403: acceso denegado al recurso.
    /// - 404: solicitud no encontrada.
    /// - 500: error interno (vía middleware).
    /// </remarks>
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

    /// <summary>
    /// Cambia el estado de una solicitud (flujo del asesor).
    /// </summary>
    /// <param name="id">Identificador de la solicitud.</param>
    /// <param name="request">Nuevo estado y observación opcional.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Solicitud actualizada con el nuevo estado.</returns>
    /// <remarks>
    /// Roles requeridos: Asesor.
    /// Códigos de respuesta:
    /// - 200: estado actualizado.
    /// - 400: transición inválida o validación (vía middleware).
    /// - 401: sin autenticación o token inválido.
    /// - 403: autenticado pero sin rol Asesor.
    /// - 404: solicitud no encontrada.
    /// - 500: error interno (vía middleware).
    /// </remarks>
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

/// <summary>
/// Contrato HTTP para crear una solicitud de apoyo.
/// </summary>
/// <param name="EstudianteId">
/// Identificador del estudiante. Obligatorio para el asesor;
/// el estudiante autenticado puede omitirlo (se resuelve por su usuario).
/// </param>
/// <param name="TipoApoyo">Tipo de apoyo solicitado.</param>
/// <param name="MontoSolicitado">Monto numérico solicitado.</param>
/// <param name="Descripcion">Descripción o justificación de la solicitud.</param>
public sealed record CreateSolicitudRequest(
    Guid? EstudianteId,
    TipoApoyo TipoApoyo,
    decimal MontoSolicitado,
    string Descripcion);

/// <summary>
/// Contrato HTTP para cambiar el estado de una solicitud.
/// </summary>
/// <param name="Estado">Nuevo estado de la solicitud.</param>
/// <param name="Observacion">Observación opcional del cambio de estado.</param>
public sealed record ChangeSolicitudStatusRequest(
    EstadoSolicitud Estado,
    string? Observacion);
