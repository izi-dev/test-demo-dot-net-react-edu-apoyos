using EduApoyos.Application.Common;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Exceptions;
using EduApoyos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del puerto del agregado SolicitudApoyo.
/// </summary>
public sealed class SolicitudRepository(EduApoyosDbContext context) : ISolicitudRepository
{
    public Task<SolicitudApoyo?> ObtenerPorIdConDetalleAsync(Guid id, CancellationToken cancellationToken = default) =>
        QueryConDetalle().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<PagedResult<SolicitudApoyo>> ListarAsync(FiltroSolicitudes filtro, CancellationToken cancellationToken = default)
    {
        var query = QueryConDetalle();

        if (filtro.Estado.HasValue)
        {
            query = query.Where(x => x.Estado == filtro.Estado.Value);
        }

        if (filtro.TipoApoyo.HasValue)
        {
            query = query.Where(x => x.TipoApoyo == filtro.TipoApoyo.Value);
        }

        if (filtro.Desde.HasValue)
        {
            query = query.Where(x => x.FechaSolicitud >= filtro.Desde.Value);
        }

        if (filtro.Hasta.HasValue)
        {
            query = query.Where(x => x.FechaSolicitud <= filtro.Hasta.Value);
        }

        query = query.OrderByDescending(x => x.FechaSolicitud);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filtro.Pagina - 1) * filtro.TamanoPagina)
            .Take(filtro.TamanoPagina)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<SolicitudApoyo>(items, filtro.Pagina, filtro.TamanoPagina, total);
    }

    public async Task<IReadOnlyCollection<SolicitudApoyo>> ListarPorEstudianteAsync(Guid estudianteId, CancellationToken cancellationToken = default) =>
        await QueryConDetalle()
            .Where(x => x.EstudianteId == estudianteId)
            .OrderByDescending(x => x.FechaSolicitud)
            .ToArrayAsync(cancellationToken);

    public async Task AgregarAsync(SolicitudApoyo solicitud, CancellationToken cancellationToken = default) =>
        await context.SolicitudesApoyo.AddAsync(solicitud, cancellationToken);

    public async Task PersistirCambioEstadoAsync(
        SolicitudApoyo solicitud,
        HistorialEstado nuevoHistorial,
        CancellationToken cancellationToken = default)
    {
        // Actualización escalar: evita el grafo Asesor/Usuario/Historial del change tracker.
        var actualizadas = await context.SolicitudesApoyo
            .Where(x => x.Id == solicitud.Id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.Estado, solicitud.Estado)
                    .SetProperty(x => x.FechaActualizacion, solicitud.FechaActualizacion)
                    .SetProperty(x => x.AsesorId, solicitud.AsesorId),
                cancellationToken);

        if (actualizadas == 0)
        {
            throw new RecursoNoEncontradoException("solicitud", solicitud.Id);
        }

        // El change tracker puede haber marcado la solicitud/historial al mutar el agregado.
        // Limpiamos y solo insertamos el nuevo historial.
        context.ChangeTracker.Clear();
        await context.HistorialEstados.AddAsync(nuevoHistorial, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<SolicitudApoyo> QueryConDetalle() =>
        context.SolicitudesApoyo
            .Include(x => x.Estudiante)
                .ThenInclude(x => x!.Usuario)
            .Include(x => x.Historial);
}
