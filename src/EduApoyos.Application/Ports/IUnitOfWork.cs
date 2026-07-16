namespace EduApoyos.Application.Ports;

/// <summary>
/// Coordina la confirmación transaccional de cambios de persistencia.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
