using EduApoyos.Application.Common;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Ports;

/// <summary>
/// Puerto de persistencia del agregado <see cref="SolicitudApoyo"/>.
/// </summary>
public interface ISolicitudRepository
{
    Task<SolicitudApoyo?> ObtenerPorIdConDetalleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<SolicitudApoyo>> ListarAsync(FiltroSolicitudes filtro, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<SolicitudApoyo>> ListarPorEstudianteAsync(Guid estudianteId, CancellationToken cancellationToken = default);
    Task AgregarAsync(SolicitudApoyo solicitud, CancellationToken cancellationToken = default);
}

/// <summary>
/// Criterios de búsqueda para el listado de solicitudes del asesor.
/// </summary>
public sealed record FiltroSolicitudes(
    EstadoSolicitud? Estado,
    TipoApoyo? TipoApoyo,
    DateTime? Desde,
    DateTime? Hasta,
    int Pagina = 1,
    int TamanoPagina = 10);
