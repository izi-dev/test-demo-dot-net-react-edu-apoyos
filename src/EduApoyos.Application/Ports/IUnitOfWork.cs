namespace EduApoyos.Application.Ports;

/// <summary>
/// Coordina la confirmación transaccional de cambios de persistencia.
/// </summary>
/// <remarks>
/// Los repositorios de aplicación suelen solo marcar entidades; este puerto
/// confirma el lote de cambios pendientes en una sola unidad de trabajo.
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>
    /// Confirma todos los cambios pendientes en el contexto de persistencia.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Número de entidades afectadas por la confirmación.</returns>
    /// <remarks>
    /// Debe ejecutarse después de operaciones como <c>AgregarAsync</c> que no persisten por sí solas.
    /// Algunas operaciones especiales (p. ej. <see cref="ISolicitudRepository.PersistirCambioEstadoAsync"/>)
    /// pueden confirmar internamente y no depender de este método.
    /// </remarks>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
