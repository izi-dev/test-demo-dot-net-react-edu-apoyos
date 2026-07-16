using EduApoyos.Application.Common;
using EduApoyos.Application.Common.Abstractions;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Enums;

namespace EduApoyos.Application.Features.Solicitudes.List;

/// <summary>
/// Consulta paginada de solicitudes para el panel del asesor.
/// </summary>
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
public sealed class ListSolicitudesQueryHandler(
    ISolicitudRepository solicitudes) : IQueryHandler<ListSolicitudesQuery, PagedResult<SolicitudDto>>
{
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
