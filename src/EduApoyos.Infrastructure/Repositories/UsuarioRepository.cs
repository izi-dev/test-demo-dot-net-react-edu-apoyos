using EduApoyos.Application.Ports;
using EduApoyos.Domain.Entities;
using EduApoyos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del puerto de usuarios del dominio.
/// </summary>
public sealed class UsuarioRepository(EduApoyosDbContext context) : IUsuarioRepository
{
    public Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Usuarios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
        await context.Usuarios.AddAsync(usuario, cancellationToken);
}
