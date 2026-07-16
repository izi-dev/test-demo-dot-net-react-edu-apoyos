using EduApoyos.Domain.Entities;

namespace EduApoyos.Application.Ports;

/// <summary>
/// Puerto de lectura de usuarios del dominio sincronizados con Identity.
/// </summary>
/// <remarks>
/// Representa la proyección de dominio del usuario (no el store de ASP.NET Identity).
/// Se usa para validar existencia de asesores/creadores y para asociar perfiles de estudiante.
/// </remarks>
public interface IUsuarioRepository
{
    /// <summary>
    /// Obtiene un usuario de dominio por su identificador.
    /// </summary>
    /// <param name="id">Identificador único del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>El usuario si existe; de lo contrario <c>null</c>.</returns>
    /// <remarks>
    /// No lanza excepción ante ausencia del recurso. La autorización y el mapeo a Identity
    /// quedan fuera de este puerto.
    /// </remarks>
    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un usuario de dominio para inserción en el almacén de persistencia.
    /// </summary>
    /// <param name="usuario">Entidad de usuario de dominio a persistir.</param>
    /// <param name="cancellationToken">Token de cancelación de la operación asíncrona.</param>
    /// <returns>Una tarea que representa el encolado de la inserción.</returns>
    /// <remarks>
    /// No confirma la transacción; requiere <see cref="IUnitOfWork.SaveChangesAsync"/>
    /// (o el flujo de Identity que lo invoque) para materializar el cambio.
    /// </remarks>
    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
