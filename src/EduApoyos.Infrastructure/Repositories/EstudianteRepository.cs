using EduApoyos.Application.Common;
using EduApoyos.Application.Ports;
using EduApoyos.Domain.Entities;
using EduApoyos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduApoyos.Infrastructure.Repositories;

/// <summary>
/// Implementación EF Core del puerto de estudiantes.
/// </summary>
public sealed class EstudianteRepository(EduApoyosDbContext context) : IEstudianteRepository
{
    public Task<Estudiante?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Estudiante?> ObtenerPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        Query().FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);

    public async Task<PagedResult<Estudiante>> ListarAsync(int pagina, int tamanoPagina, CancellationToken cancellationToken = default)
    {
        var query = Query().OrderBy(x => x.Usuario!.NombreCompleto);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pagina - 1) * tamanoPagina).Take(tamanoPagina).ToArrayAsync(cancellationToken);
        return new PagedResult<Estudiante>(items, pagina, tamanoPagina, total);
    }

    public async Task AgregarAsync(Estudiante estudiante, CancellationToken cancellationToken = default) =>
        await context.Estudiantes.AddAsync(estudiante, cancellationToken);

    public Task<bool> ExisteUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        context.Estudiantes.AnyAsync(x => x.UsuarioId == usuarioId, cancellationToken);

    private IQueryable<Estudiante> Query() =>
        context.Estudiantes.Include(x => x.Usuario);
}
