using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Features.Solicitudes;

/// <summary>
/// Proyección de lectura de una solicitud de apoyo para la API y el frontend.
/// </summary>
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
internal static class SolicitudMapper
{
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
