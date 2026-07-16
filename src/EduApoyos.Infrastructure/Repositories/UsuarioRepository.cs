using EduApoyos.Application.Ports;
using EduApoyos.Domain.Entities;
using EduApoyos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del puerto <see cref="IUsuarioRepository"/>.
/// </summary>
public sealed class UsuarioRepository(EduApoyosDbContext context) : IUsuarioRepository
{
    /// <summary>
    /// Obtiene un usuario de dominio por su identificador.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El usuario si existe; de lo contrario, <c>null</c>.</returns>
    public Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Usuarios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    /// <summary>
    /// Agrega un usuario de dominio al contexto (pendiente de <c>SaveChanges</c>).
    /// </summary>
    /// <param name="usuario">Entidad de usuario a insertar.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Una tarea que completa cuando la entidad queda rastreada para inserción.</returns>
    public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
        await context.Usuarios.AddAsync(usuario, cancellationToken);
}
