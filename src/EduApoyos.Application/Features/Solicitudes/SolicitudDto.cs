using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Features.Solicitudes;

/// <summary>
/// Proyección de lectura de una solicitud de apoyo para la API y el frontend.
/// </summary>
/// <param name="Id">Identificador de la solicitud.</param>
/// <param name="EstudianteId">Identificador del estudiante solicitante.</param>
/// <param name="NombreEstudiante">Nombre completo del estudiante (desde navegación Usuario).</param>
/// <param name="TipoApoyo">Tipo de apoyo económico solicitado.</param>
/// <param name="MontoSolicitado">Monto solicitado.</param>
/// <param name="Descripcion">Descripción de la solicitud.</param>
/// <param name="Estado">Estado actual del flujo institucional.</param>
/// <param name="FechaSolicitud">Fecha de creación.</param>
/// <param name="FechaActualizacion">Fecha del último cambio de estado o actualización.</param>
/// <param name="AsesorId">Asesor asignado, si existe.</param>
/// <param name="Historial">Historial de cambios de estado ordenado cronológicamente.</param>
public sealed record SolicitudDto(
    Guid Id,
    Guid EstudianteId,
    string NombreEstudiante,
    TipoApoyo TipoApoyo,
    decimal MontoSolicitado,
    string Descripcion,
    EstadoSolicitud Estado,
    DateTime FechaSolicitud,
    DateTime FechaActualizacion,
    Guid? AsesorId,
    IReadOnlyCollection<HistorialEstadoDto> Historial);

/// <summary>
/// Proyección de un cambio de estado dentro del historial de una solicitud.
/// </summary>
/// <param name="Id">Identificador del registro de historial.</param>
/// <param name="EstadoAnterior">Estado previo; <c>null</c> en el registro inicial de creación.</param>
/// <param name="EstadoNuevo">Estado resultante del cambio.</param>
/// <param name="FechaCambio">Momento en que se registró el cambio.</param>
/// <param name="UsuarioId">Usuario que efectuó el cambio.</param>
/// <param name="Observacion">Observación asociada al cambio (puede ser cadena vacía).</param>
public sealed record HistorialEstadoDto(
    Guid Id,
    EstadoSolicitud? EstadoAnterior,
    EstadoSolicitud EstadoNuevo,
    DateTime FechaCambio,
    Guid UsuarioId,
    string Observacion);

/// <summary>
/// Mapea agregados de dominio a DTOs de lectura.
/// </summary>
/// <remarks>
/// Uso interno de la capa de aplicación. Si falta la navegación de estudiante/usuario,
/// <see cref="SolicitudDto.NombreEstudiante"/> se proyecta como cadena vacía.
/// El historial se ordena por <c>FechaCambio</c> ascendente.
/// </remarks>
internal static class SolicitudMapper
{
    /// <summary>
    /// Convierte un agregado <see cref="SolicitudApoyo"/> en <see cref="SolicitudDto"/>.
    /// </summary>
    /// <param name="solicitud">Agregado de dominio con detalle opcionalmente cargado.</param>
    /// <returns>DTO de lectura listo para la API.</returns>
    public static SolicitudDto ToDto(SolicitudApoyo solicitud) =>
        new(
            Id: solicitud.Id,
            EstudianteId: solicitud.EstudianteId,
            NombreEstudiante: solicitud.Estudiante?.Usuario?.NombreCompleto ?? string.Empty,
            TipoApoyo: solicitud.TipoApoyo,
            MontoSolicitado: solicitud.MontoSolicitado,
            Descripcion: solicitud.Descripcion,
            Estado: solicitud.Estado,
            FechaSolicitud: solicitud.FechaSolicitud,
            FechaActualizacion: solicitud.FechaActualizacion,
            AsesorId: solicitud.AsesorId,
            Historial: solicitud.Historial
                .OrderBy(x => x.FechaCambio)
                .Select(x => new HistorialEstadoDto(
                    Id: x.Id,
                    EstadoAnterior: x.EstadoAnterior,
                    EstadoNuevo: x.EstadoNuevo,
                    FechaCambio: x.FechaCambio,
                    UsuarioId: x.UsuarioId,
                    Observacion: x.Observacion))
                .ToArray());
}
