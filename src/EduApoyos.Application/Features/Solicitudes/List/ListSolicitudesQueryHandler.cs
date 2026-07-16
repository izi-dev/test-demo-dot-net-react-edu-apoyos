using EduApoyos.Application.Common;
using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Features.Solicitudes.List;

/// <summary>
/// Consulta paginada de solicitudes para el panel del asesor.
/// </summary>
/// <param name="Estado">Filtro opcional por estado.</param>
/// <param name="TipoApoyo">Filtro opcional por tipo de apoyo.</param>
/// <param name="Desde">Fecha mínima inclusiva de solicitud.</param>
/// <param name="Hasta">Fecha máxima inclusiva de solicitud.</param>
/// <param name="Pagina">Número de página (base 1). Valores &lt; 1 se normalizan a 1.</param>
/// <param name="TamanoPagina">
/// Tamaño de página. Valores &lt; 1 o &gt; 100 se normalizan a 10.
/// </param>
public sealed record ListSolicitudesQuery(
    EstadoSolicitud? Estado,
    TipoApoyo? TipoApoyo,
    DateTime? Desde,
    DateTime? Hasta,
    int Pagina = 1,
    int TamanoPagina = 10);

/// <summary>
/// Lista solicitudes aplicando filtros y paginación.
/// </summary>
/// <remarks>
/// No tiene validator FluentValidation; normaliza página/tamaño y delega en
/// <see cref="ISolicitudRepository.ListarAsync"/> con un <see cref="FiltroSolicitudes"/>.
/// </remarks>
/// <param name="solicitudes">Puerto de lectura de solicitudes.</param>
public sealed class ListSolicitudesQueryHandler(
    ISolicitudRepository solicitudes) : IQueryHandler<ListSolicitudesQuery, PagedResult<SolicitudDto>>
{
    /// <summary>
    /// Obtiene una página de solicitudes proyectada a <see cref="SolicitudDto"/>.
    /// </summary>
    /// <param name="query">Filtros y paginación.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>
    /// <see cref="PagedResult{T}"/> de <see cref="SolicitudDto"/>. Página vacía si no hay coincidencias.
    /// </returns>
    public async Task<PagedResult<SolicitudDto>> HandleAsync(ListSolicitudesQuery query, CancellationToken cancellationToken = default)
    {
        var pagina = query.Pagina < 1 ? 1 : query.Pagina;
        var tamano = query.TamanoPagina is < 1 or > 100 ? 10 : query.TamanoPagina;

        var resultado = await solicitudes.ListarAsync(
            new FiltroSolicitudes(
                Estado: query.Estado,
                TipoApoyo: query.TipoApoyo,
                Desde: query.Desde,
                Hasta: query.Hasta,
                Pagina: pagina,
                TamanoPagina: tamano),
            cancellationToken);

        return new PagedResult<SolicitudDto>(
            Items: resultado.Items.Select(SolicitudMapper.ToDto).ToArray(),
            Page: resultado.Page,
            PageSize: resultado.PageSize,
            TotalItems: resultado.TotalItems);
    }
}
