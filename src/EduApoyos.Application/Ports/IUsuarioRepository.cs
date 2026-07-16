using EduApoyos.Domain.Entities;

namespace EduApoyos.Application.Ports;

/// <summary>
/// Puerto de lectura de usuarios del dominio sincronizados con Identity.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
