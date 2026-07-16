using EduApoyos.Application.Common;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Entities;
using EduApoyos.Domain.Exceptions;
using EduApoyos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del puerto <see cref="ISolicitudRepository"/>
/// para el agregado <see cref="SolicitudApoyo"/>.
/// </summary>
public sealed class SolicitudRepository(EduApoyosDbContext context) : ISolicitudRepository
{
    /// <summary>
    /// Obtiene una solicitud por identificador, incluyendo estudiante, usuario e historial.
    /// </summary>
    /// <param name="id">Identificador de la solicitud.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>La solicitud con detalle si existe; de lo contrario, <c>null</c>.</returns>
    public Task<SolicitudApoyo?> ObtenerPorIdConDetalleAsync(Guid id, CancellationToken cancellationToken = default) =>
        QueryConDetalle().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    /// <summary>
    /// Lista solicitudes filtradas y paginadas, ordenadas por fecha de solicitud descendente.
    /// </summary>
    /// <param name="filtro">Filtros opcionales de estado, tipo, rango de fechas y paginación.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Resultado paginado de solicitudes con detalle.</returns>
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

    /// <summary>
    /// Lista todas las solicitudes de un estudiante, más recientes primero.
    /// </summary>
    /// <param name="estudianteId">Identificador del estudiante.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Colección de solo lectura de solicitudes con detalle.</returns>
    public async Task<IReadOnlyCollection<SolicitudApoyo>> ListarPorEstudianteAsync(Guid estudianteId, CancellationToken cancellationToken = default) =>
        await QueryConDetalle()
            .Where(x => x.EstudianteId == estudianteId)
            .OrderByDescending(x => x.FechaSolicitud)
            .ToArrayAsync(cancellationToken);

    /// <summary>
    /// Agrega una solicitud al contexto (pendiente de <c>SaveChanges</c>).
    /// </summary>
    /// <param name="solicitud">Agregado de solicitud a insertar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Una tarea que completa cuando la entidad queda rastreada para inserción.</returns>
    public async Task AgregarAsync(SolicitudApoyo solicitud, CancellationToken cancellationToken = default) =>
        await context.SolicitudesApoyo.AddAsync(solicitud, cancellationToken);

    /// <summary>
    /// Persiste un cambio de estado con actualización escalar y un nuevo registro de historial.
    /// Evita conflictos del change tracker con el grafo Asesor/Usuario/Historial.
    /// </summary>
    /// <param name="solicitud">Solicitud ya mutada en memoria (estado, fecha y asesor).</param>
    /// <param name="nuevoHistorial">Entrada de historial a insertar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Una tarea que completa cuando la actualización y el historial se guardan.</returns>
    /// <exception cref="RecursoNoEncontradoException">
    /// Se lanza si no existe una fila de solicitud con el identificador indicado.
    /// </exception>
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

    /// <summary>
    /// Consulta base de solicitudes con estudiante, usuario del estudiante e historial.
    /// </summary>
    /// <returns>Consulta IQueryable lista para filtrar o materializar.</returns>
    private IQueryable<SolicitudApoyo> QueryConDetalle() =>
        context.SolicitudesApoyo
            .Include(x => x.Estudiante)
                .ThenInclude(x => x!.Usuario)
            .Include(x => x.Historial);
}
